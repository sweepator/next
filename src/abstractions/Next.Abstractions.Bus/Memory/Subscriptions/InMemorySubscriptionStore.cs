using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Next.Abstractions.Bus.Subscriptions;

namespace Next.Abstractions.Bus.Memory.Subscriptions
{
    /// <summary>
    /// In-process subscription store
    /// </summary>
    public class InMemorySubscriptionStore : ISubscriptionStore
    {
        private readonly HashSet<Subscription> _subscriptions;
        private readonly object _subscriptionsLock = new object();

        public InMemorySubscriptionStore()
        {
            _subscriptions = new HashSet<Subscription>();
        }

        public Task<IEnumerable<Subscription>> GetAll()
        {
            lock (_subscriptionsLock)
            {
                IEnumerable<Subscription> result = _subscriptions.ToArray();
                return Task.FromResult(result);
            }
        }

        public Task<IEnumerable<Subscription>> GetByTopic(string topic)
        {
            lock (_subscriptionsLock)
            {
                IEnumerable<Subscription> result = (from subscription in _subscriptions
                    where subscription.Topic == topic
                    select subscription).ToArray();

                return Task.FromResult(result);
            }
        }

        public Task<IEnumerable<Subscription>> GetByEndpoint(string endpoint)
        {
            lock (_subscriptionsLock)
            {
                IEnumerable<Subscription> result = (from subscription in _subscriptions
                    where subscription.Endpoint == endpoint
                    select subscription).ToArray();

                return Task.FromResult(result);
            }
        }

        public Task<bool> AddSubscription(Subscription subscription)
        {
            lock (_subscriptionsLock)
            {
                if (!_subscriptions.Contains(subscription))
                {
                    _subscriptions.Add(subscription);
                    
                    return Task.FromResult(true);
                }

                return Task.FromResult(false);
            }
        }

        public Task<bool> RemoveSubscription(Subscription subscription)
        {
            lock (_subscriptionsLock)
            {
                if (_subscriptions.Contains(subscription))
                {
                    _subscriptions.Remove(subscription);

                    return Task.FromResult(true);
                }

                return Task.FromResult(false);
            }
        }
    }
}