using System.Collections.Generic;
using System.Linq;

namespace Next.Abstractions.Bus.Subscriptions
{
    /// <summary>
    /// Helper structure to manage subscription changes
    /// </summary>
    class SubscriptionSet
    {
        private readonly HashSet<Subscription> _subscriptions;

        // readonly copy of the current subscriptions to allow other threads to read them while the HashSet subscriptions is modified by others
        private Subscription[] _latestCopy;

        public SubscriptionSet()
        {
            _subscriptions = new HashSet<Subscription>();
            _latestCopy = new Subscription[0];
        }

        public Subscription[] Subscriptions => _latestCopy;

        public void Add(Subscription subscription)
        {
            if (!_subscriptions.Contains(subscription))
            {
                _subscriptions.Add(subscription);
                _latestCopy = _subscriptions.ToArray();
            }
        }

        public void Remove(Subscription subscription)
        {
            if (_subscriptions.Contains(subscription))
            {
                _subscriptions.Remove(subscription);
                _latestCopy = _subscriptions.ToArray();
            }
        }
    }
}