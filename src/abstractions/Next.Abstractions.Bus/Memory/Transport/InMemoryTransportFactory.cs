using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Next.Abstractions.Bus.Transport;

namespace Next.Abstractions.Bus.Memory.Transport
{
    public class InMemoryTransportFactory : BrokerlessTransportFactory
    {
        // shared bus, for all local clients
        private readonly InMemoryBus _bus;

        public InMemoryTransportFactory()
        {
            _bus = new InMemoryBus();
        }

        protected override IInboundTransport CreateInboundTransport(string endpoint)
        {
            var queue = _bus.GetEndpointQueue(endpoint);

            return new InMemoryInboundTransport(queue);
        }

        protected override IOutboundTransport CreateOutboundTransport()
        {
            return new InMemoryOutboundTransport(_bus);
        }

        private class InMemoryBus
        {
            private readonly ConcurrentDictionary<string, BlockingCollection<InMemoryMessage>> _bus;

            public InMemoryBus()
            {
                _bus = new ConcurrentDictionary<string, BlockingCollection<InMemoryMessage>>();
            }

            public BlockingCollection<InMemoryMessage> GetEndpointQueue(string endpoint)
            {
                var queue = _bus.GetOrAdd(endpoint, 
                    e => new BlockingCollection<InMemoryMessage>(
                    new ConcurrentQueue<InMemoryMessage>()));

                return queue;
            }
        }

        private class InMemoryInboundTransport : IInboundTransport
        {
            private readonly BlockingCollection<InMemoryMessage> _queue;

            public InMemoryInboundTransport(BlockingCollection<InMemoryMessage> queue)
            {
                _queue = queue;
            }

            public Task<IMessageTransaction> Receive(TimeSpan timeout)
            {
                return Task.Factory.StartNew(() =>
                {
                    if (!_queue.TryTake(out var message, timeout))
                    {
                        return null;
                    }

                    IMessageTransaction transaction = new InMemoryMessageTransaction(
                        message,
                        () => { }, // nothing to do on commit
                        () =>
                        {
                            message.Failed(); // notify failure, increases retry count
                            _queue.Add(message); // add to the queue again
                        });
                    return transaction;
                });
            }
        }

        private class InMemoryOutboundTransport : IOutboundTransport
        {
            private readonly InMemoryBus _bus;

            public InMemoryOutboundTransport(InMemoryBus bus)
            {
                _bus = bus;
            }

            public Task Send(TransportMessage message)
            {
                var endpoint = message.Headers[MessageHeaders.Endpoint];
                var endpointQueue = _bus.GetEndpointQueue(endpoint);

                endpointQueue.Add(new InMemoryMessage(message));
                return Task.FromResult(true);
            }

            public Task SendMultiple(IEnumerable<TransportMessage> messages)
            {
                foreach (var message in messages)
                {
                    var endpoint = message.Headers[MessageHeaders.Endpoint];
                    var endpointQueue = _bus.GetEndpointQueue(endpoint);

                    endpointQueue.Add(new InMemoryMessage(message));
                }

                return Task.FromResult(true);
            }
        }

        private class InMemoryMessage
        {
            private int _deliveryCount;
            
            public TransportMessage Message { get; }

            public int DeliveryCount => Thread.VolatileRead(ref _deliveryCount);

            public InMemoryMessage(TransportMessage message)
            {
                Message = message;
                _deliveryCount = 1;
            }
            
            public void Failed()
            {
                Interlocked.Increment(ref _deliveryCount);
            }
        }

        private class InMemoryMessageTransaction : IMessageTransaction
        {
            private readonly Action _onCommit;
            private readonly Action _onFail;
            private readonly InMemoryMessage _inMemoryMsg;

            public InMemoryMessageTransaction(
                InMemoryMessage message, 
                Action onCommit,
                Action onFail)
            {
                _inMemoryMsg = message;
                _onCommit = onCommit;
                _onFail = onFail;
            }

            public TransportMessage Message => _inMemoryMsg.Message;

            public int DeliveryCount => _inMemoryMsg.DeliveryCount;

            public Task Commit()
            {
                _onCommit();
                return Task.FromResult(true);
            }

            public Task Fail()
            {
                _onFail();
                return Task.FromResult(true);
            }
        }
    }
}