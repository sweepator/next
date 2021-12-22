using System;
using System.Threading.Tasks;
using Next.Abstractions.Bus.Transport;

namespace Next.Bus.Redis.Transport
{
    public class RedisMessageTransaction : IMessageTransaction
    {
        private readonly Func<Task> _onCommit;
        private readonly Func<Task> _onFail;

        public RedisMessageTransaction(
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