using System;
using System.Threading.Tasks;
using Next.Abstractions.Bus.Transport;

namespace Next.Bus.MassTransit.Transport
{
    internal class MessageTransaction : IMessageTransaction
    {
        private readonly Func<Task> _onCommit;
        private readonly Func<Task> _onFail;

        public MessageTransaction(
            TransportMessage message, 
            int deliveryCount, 
            Func<Task> onCommit, 
            Func<Task> onFail)
        {
            Message = message;
            DeliveryCount = deliveryCount;
            _onCommit = onCommit;
            _onFail = onFail;
        }

        public TransportMessage Message { get; }

        public int DeliveryCount { get; }

        public Task Commit()
        {
            return _onCommit();
        }

        public Task Fail()  
        {
            return _onFail();
        }
    }
}