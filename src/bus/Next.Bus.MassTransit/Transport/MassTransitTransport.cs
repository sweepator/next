using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using Next.Abstractions.Bus.Transport;
using Next.Bus.MassTransit.Configuration;

namespace Next.Bus.MassTransit.Transport
{
    internal class MassTransitTransport: 
        IInboundTransport,
        IOutboundTransport,
        IConsumer<TransportMessage>
    {
        private readonly ISendEndpointProvider _sendEndpointProvider;
        private readonly ISendEndpointStrategy _sendEndpointStrategy;
        private readonly MassTransitBusConfiguration _configuration;
        private readonly string _endpoint;

        private static readonly ConcurrentDictionary<string, ConcurrentQueue<MessageTransaction>> Queue = new();

        public MassTransitTransport(
            ISendEndpointProvider sendEndpointProvider,
            ISendEndpointStrategy sendEndpointStrategy,
            MassTransitBusConfiguration configuration,
            string endpoint = null)
        {
            _sendEndpointProvider = sendEndpointProvider;
            _sendEndpointStrategy = sendEndpointStrategy;
            _configuration = configuration;
            _endpoint = endpoint;

            if (!string.IsNullOrEmpty(_endpoint))
            {
                Queue.TryAdd(_endpoint, new ConcurrentQueue<MessageTransaction>());
            }
        }
        
        public async Task Send(TransportMessage message)
        {
            await InternalSend(
                message,
                0);
        }

        public async Task SendMultiple(IEnumerable<TransportMessage> messages)
        {
            foreach (var message in messages)
            {
                await Send(message);
            }
        }
        
        public async Task<IMessageTransaction> Receive(TimeSpan timeout)
        {
            var sw = Stopwatch.StartNew();
            while (sw.Elapsed < timeout)
            {
                if (Queue[_endpoint].TryDequeue(out var messageTransaction))
                {
                    return messageTransaction;
                }
                await Task.Delay(50);
            }
  
            return null;
        }

        public async Task Consume(ConsumeContext<TransportMessage> context)
        {
            var deliveryCount = context.Headers
                .Get<int>(MassTransitMessageHeaders.DeliveryCount)
                .GetValueOrDefault();

            var endpoint = context.Message.Headers[Abstractions.Bus.MessageHeaders.Endpoint];
            var message = context.Message;
            
            var messageTransaction = new MessageTransaction(
                context.Message,
                deliveryCount,
                async () => await CommitTransaction(),
                async () => await FailTransaction(
                    message,
                    deliveryCount));
            
            Queue[endpoint].Enqueue(messageTransaction);
            
            var sw = Stopwatch.StartNew();
            while (IsMessagesQueued(messageTransaction) 
                   && sw.Elapsed < _configuration.ConsumerTimeout)
            {
                await Task.Delay(50);
            }

            if (IsMessagesQueued(messageTransaction))
            {
                throw new TimeoutException($"Message id {messageTransaction.Message.Id} processing timeout");
            }
        }

        private static Task CommitTransaction()
        {
            return Task.CompletedTask;
        }

        private async Task FailTransaction(
            TransportMessage transportMessage,
            int deliveryCount)
        {
            deliveryCount++;
            await InternalSend(
                transportMessage,
                deliveryCount);
        }

        private async Task InternalSend(
            TransportMessage message,
            int deliveryCount)
        {
            var endpoint = _sendEndpointStrategy.GetProducerEndpoint(message);
            var sendEndpoint = await _sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{endpoint}"));

            await sendEndpoint.Send(message,
                context =>
                {
                    context.Headers.Set(
                        MassTransitMessageHeaders.DeliveryCount, 
                        deliveryCount.ToString());
                });
        }

        private static bool IsMessagesQueued(MessageTransaction messageTransaction)
        {
            var endpoint = messageTransaction.Message.Headers[Abstractions.Bus.MessageHeaders.Endpoint];
            return Queue.ContainsKey(endpoint) && Queue[endpoint].Contains(messageTransaction);
        }
    }
}