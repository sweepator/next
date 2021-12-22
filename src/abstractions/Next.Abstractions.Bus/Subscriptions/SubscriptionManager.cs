using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Next.Abstractions.Bus.Subscriptions
{
    /// <summary>
    /// SubscriptionManager is intended to use SubscriptionStore to store and cache subscriptions
    /// </summary>
    public class SubscriptionManager : ISubscriptionManager
    {
        private readonly ISubscriptionStore _store;
        private readonly ISubscriptionBroker _broker;
        private readonly Dictionary<string, SubscriptionSet> _cache;
        private readonly object _cacheLock;
        private readonly Lazy<Task> _initializationTask;

        public SubscriptionManager(
            ISubscriptionStore store, 
            ISubscriptionBroker broker)
        {
            _store = store;
            _broker = broker;
            _cache = new Dictionary<string, SubscriptionSet>();
            _cacheLock = new object();
            _initializationTask = new Lazy<Task>(Initialize);
        }

        public async Task<IEnumerable<Subscription>> GetSubscriptions(string topic)
        {
            // lazy initialization, ensures only one thread runs the initialize function
            await _initializationTask.Value; 

            lock (_cacheLock)
            {
                var set = _cache.GetOrAdd(topic, t => new SubscriptionSet());
                return set.Subscriptions;
            }
        }

        public async Task UpdateEndpointSubscriptions(string endpoint, IEnumerable<Subscription> subscriptions)
        {
            // lazy initialization, ensures only one thread runs the initialize function
            await _initializationTask.Value;

            var storedSubscriptions = await _store.GetByEndpoint(endpoint);

            var subscriptionsToAdd = subscriptions.Except(storedSubscriptions);
            var subscriptionsToRemove = storedSubscriptions.Except(subscriptions);

            var tasks = subscriptionsToAdd.Select(AddSubscription).ToList();

            tasks.AddRange(subscriptionsToRemove.Select(RemoveSubscription));

            if (tasks.Count > 0)
            {
                await Task.WhenAll(tasks.ToArray());
            }
        }

        private async Task AddSubscription(Subscription subscription)
        {
            var success = await _store.AddSubscription(subscription);
            if (success)
            {
                // update current cache without waiting for broker notification to be received
                OnSubscriptionAdded(subscription);

                //notify other processes of this change
                await _broker.NotifyChange(SubscriptionChange.Add, subscription);
            }
        }

        private async Task RemoveSubscription(Subscription subscription)
        {
            var success = await _store.RemoveSubscription(subscription);
            if (success)
            {
                // update current cache
                OnSubscriptionRemoved(subscription);

                //notify other processes of this change
                await _broker.NotifyChange(SubscriptionChange.Remove, subscription);
            }
        }

        private async Task Initialize()
        {
            await _broker.SubscribeChangeNotifications(OnSubscriptionChanged);

            var subscriptions = await _store.GetAll();

            lock (_cacheLock)
            {
                foreach (var subscription in subscriptions)
                {
                    var set = _cache.GetOrAdd(subscription.Topic, s => new SubscriptionSet());
                    set.Add(subscription);
                }
            }
        }

        private void OnSubscriptionChanged(SubscriptionChange change, Subscription subscription)
        {
            if (change == SubscriptionChange.Add)
            {
                OnSubscriptionAdded(subscription);
            }
            else
            {
                OnSubscriptionRemoved(subscription);
            }
        }

        private void OnSubscriptionAdded(Subscription subscription)
        {
            lock (_cacheLock)
            {
                var set = _cache.GetOrAdd(subscription.Topic, s => new SubscriptionSet());
                set.Add(subscription);
            }
        }

        private void OnSubscriptionRemoved(Subscription subscription)
        {
            lock (_cacheLock)
            {
                var set = _cache.GetOrAdd(subscription.Topic, s => new SubscriptionSet());
                set.Remove(subscription);
            }
        }
    }
}