using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Next.Abstractions.Bus.Subscriptions;
using Next.Abstractions.Bus.Transport;

namespace Next.Abstractions.Bus
{
    /// <summary>
    /// Handles all the logic to Send messages to an IOutboundTransport, based on current subscriptions
    /// Essentially takes Messages, transforms into TransportMessages, setting the appropriate headers for each subscription and sends them to an IOutboundTransport
    /// </summary>
    public class MessageBus : IMessageBus
    {
        private readonly ISubscriptionManager _subscriptionManager;
        private readonly IOutboundTransport _transport;
        private readonly IMessageSerializer _messageSerializer;
        private readonly IEnumerable<IProcessor> _processors;
        private readonly ILogger<IMessageBus> _logger;

        public MessageBus(
            ISubscriptionManager subscriptionManager, 
            IOutboundTransport transport,
            IMessageSerializer messageSerializer,
            IEnumerable<IProcessor> processors,
            ILogger<MessageBus> logger)
        {
            _subscriptionManager = subscriptionManager;
            _transport = transport;
            _messageSerializer = messageSerializer;
            _processors = processors;
            _logger = logger;
        }

        public Task Send(Message message)
        {
            return Send(new[] { message });
        }

        public async Task Send(IEnumerable<Message> messages)
        {
            var transportMessages = new List<TransportMessage>();

            foreach (var message in messages)
            {
                var subscriptions = await _subscriptionManager.GetSubscriptions(message.Name);
                if (subscriptions.Any())
                {
                    transportMessages.AddRange(subscriptions.Select(s => BuildTransportMessage(message, s)));
                }
                else
                {
                    // TODO: retry after some time?
                    _logger.LogDebug($"Attempt to send message {message.Name} but no subscriptions were found");
                }
            }

            if (!transportMessages.Any())
            {
                return;
            }

            await PublishMessages(transportMessages);
        }

        public async Task Start()
        {
            _logger.LogDebug(
                "Starting message bus processors: {ProcessorCount}",
                _processors.Count());
            foreach (var processor in _processors)
            {
                await processor.Start();
            }
            
            _logger.LogDebug("Message bus processors started");
        }

        public Task Stop()
        {
            return Task.Run(() =>
            {
                _logger.LogDebug(
                    "Stopping message bus processors: {ProcessorCount}",
                    _processors.Count());
                foreach (var processor in _processors)
                {
                    processor.Stop();
                }
                _logger.LogDebug("Message bus processors stopped");
            });
        }

        private TransportMessage BuildTransportMessage(
            Message message, 
            Subscription subscription)
        {
            var serializedMessage = _messageSerializer.Serialize(message.Payload);

            var transportMessage = new TransportMessage
            {
                // reuse existing Id or create a new one
                Id = message.Headers.GetOrDefault(MessageHeaders.Id) ?? Guid.NewGuid().ToString(),
                Name = message.Name,
                PayLoad = serializedMessage
            };
            

            // copy original headers
            foreach (var (key, value) in message.Headers)
            {
                transportMessage.Headers[key] = value;
            }
            
            // add subscription headers
            transportMessage.Headers[MessageHeaders.Endpoint] = subscription.Endpoint; // destination queue / processor
            transportMessage.Headers[MessageHeaders.Component] = subscription.Component; //handler within the processor

            _logger.LogDebug("Sending {MessageName} to {Endpoint}",
                transportMessage.Name,
                subscription.Endpoint);

            return transportMessage;
        }

        private Task PublishMessages(IEnumerable<TransportMessage> messages)
        {
            var count = messages.Count();

            if (count == 0)
            {
                return Task.FromResult(0);
            }

            if (count == 1)
            {
                var message = messages.First();
                return _transport.Send(message);
            }

            return _transport.SendMultiple(messages);
        }
    }
}