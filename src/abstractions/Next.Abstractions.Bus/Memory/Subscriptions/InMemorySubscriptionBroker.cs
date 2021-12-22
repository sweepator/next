using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Next.Abstractions.Bus.Subscriptions;

namespace Next.Abstractions.Bus.Memory.Subscriptions
{
    /// <summary>
    /// In-process subscription broker, simply loops back whatever comes
    /// </summary>
    public class InMemorySubscriptionBroker : ISubscriptionBroker
    {
        private readonly ConcurrentBag<Action<SubscriptionChange, Subscription>> _handlers;

        public InMemorySubscriptionBroker()
        {
            _handlers = new ConcurrentBag<Action<SubscriptionChange, Subscription>>();
        }

        public Task NotifyChange(SubscriptionChange changeType, Subscription subscription)
        {
            foreach (var handler in _handlers)
            {
                NotifyHandler(handler, changeType, subscription);
            }

            return Task.FromResult(1);
        }

        public Task SubscribeChangeNotifications(Action<SubscriptionChange, Subscription> handler)
        {
            _handlers.Add(handler);
            return Task.FromResult(1);
        }

        private void NotifyHandler(
            Action<SubscriptionChange, Subscription> handler, 
            SubscriptionChange change, 
            Subscription subscription)
        {
            handler(change, subscription);
        }
    }
}