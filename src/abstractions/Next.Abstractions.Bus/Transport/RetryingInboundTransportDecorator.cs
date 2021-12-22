using System;
using System.Threading.Tasks;

namespace Next.Abstractions.Bus.Transport
{
    public class RetryingInboundTransportDecorator : IInboundTransport
    {
        private readonly IInboundTransport _inbound;
        private readonly IOutboundTransport _outbound;
        private readonly InboundTransportOptions _options;

        public RetryingInboundTransportDecorator(
            IInboundTransport inbound,
            IOutboundTransport outbound,
            InboundTransportOptions options)
        {
            _inbound = inbound;
            _outbound = outbound;
            _options = options;
        }

        public async Task<IMessageTransaction> Receive(TimeSpan timeout)
        {
            var message = await _inbound.Receive(timeout);

            if (message == null || message.DeliveryCount < _options.MaxDeliveryCount)
            {
                return message;
            }

            if (message.DeliveryCount == _options.MaxDeliveryCount)
            {
                return new MessageTransactionDecorator(
                    message, 
                    _outbound, 
                    _options);
            }

            // received a message with RetryCount > MaxRetries
            // it means that the source message was sent to dead letter endpoint but not deleted on source endpoint
            
            // in this case we delete it and silently reject it
            await message.Commit();
            
            return null;
        }

        class MessageTransactionDecorator : IMessageTransaction
        {
            private readonly IMessageTransaction _source;
            private readonly IOutboundTransport _outbound;
            private readonly InboundTransportOptions _options;

            public MessageTransactionDecorator(
                IMessageTransaction source,
                IOutboundTransport outbound,
                InboundTransportOptions options)
            {
                _source = source;
                _outbound = outbound;
                _options = options;
            }

            public TransportMessage Message => _source.Message;

            public int DeliveryCount => _source.DeliveryCount;

            public Task Commit()
            {
                return _source.Commit();
            }

            public async Task Fail()
            {
                if (_options.DeadLeterMessages)
                {
                    var deadLetterMessage = BuildDeadLetterMessage();

                    // send dead letter msg to dead letter endpoint
                    await _outbound.Send(deadLetterMessage);
                }

                // delete the source message
                // in case this fails, the source message will be retried with a RetryCounter higher than max retries
                // if/when that happens the DeadLetterTransportDecorator will reject it
                await _source.Commit();
            }

            private TransportMessage BuildDeadLetterMessage()
            {
                Message.Headers[MessageHeaders.OriginalEndpoint] = Message.Headers[MessageHeaders.Endpoint];
                Message.Headers[MessageHeaders.Endpoint] = _options.GetDeadLetterEndpoint();

                return Message;
            }
        }
    }
}