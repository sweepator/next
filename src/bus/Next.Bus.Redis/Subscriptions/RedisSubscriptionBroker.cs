using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Next.Abstractions.Bus.Subscriptions;
using Next.Data.Redis;
using StackExchange.Redis;

namespace Next.Bus.Redis.Subscriptions
{
    public class RedisSubscriptionBroker : ISubscriptionBroker
    {
        private static readonly char[] Separator = { '|' };

        private readonly IRedisConnectionFactory _redisConnectionFactory;
        private readonly string _connectionString;
        private readonly string _subscriptionChannel;
        private readonly ConcurrentBag<Action<SubscriptionChange, Subscription>> _handlers;
        private readonly Lazy<Task> _initializationTask;

        public RedisSubscriptionBroker(
            IRedisConnectionFactory redisConnectionFactory,
            string connectionString,
            string prefix)
        {
            _redisConnectionFactory = redisConnectionFactory;
            _connectionString = connectionString;
            _subscriptionChannel = $"{prefix}.subscriptions.broker";
            _handlers = new ConcurrentBag<Action<SubscriptionChange, Subscription>>();
            _initializationTask = new Lazy<Task>(Initialize);
        }

        public async Task NotifyChange(
            SubscriptionChange changeType, 
            Subscription subscription)
        {
            var redis = GetConnection().GetDatabase();

            var message = $"{(int)changeType}|{subscription.Id}";
            await redis.PublishAsync(
                _subscriptionChannel, 
                message);
        }

        public async Task SubscribeChangeNotifications(Action<SubscriptionChange, Subscription> handler)
        {
            _handlers.Add(handler);

            // lazy initialization, ensures only one thread runs the initialize function
            await _initializationTask.Value; 
        }

        private async Task Initialize()
        {
            // first attempt
            var subscriber = GetConnection().GetSubscriber();

            await subscriber.SubscribeAsync(
                _subscriptionChannel, 
                (channel, message) => OnSubscriptionChanged(message));
        }
        
        private void OnSubscriptionChanged(string message)
        {
            var parts = message.Split(Separator, 2);

            var change = (SubscriptionChange)int.Parse(parts[0]);
            var subscriptionId = parts[1];

            var subscription = Subscription.FromId(subscriptionId);

            foreach (var handler in _handlers)
            {
                var h = handler;
                Task.Run(() => h(change, subscription));
            }
        }
        
        private ConnectionMultiplexer GetConnection() => _redisConnectionFactory.GetConnection(_connectionString);
    }
}