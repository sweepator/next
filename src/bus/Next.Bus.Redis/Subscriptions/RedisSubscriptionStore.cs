using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Next.Abstractions.Bus.Subscriptions;
using Next.Data.Redis;
using StackExchange.Redis;

namespace Next.Bus.Redis.Subscriptions
{
    public class RedisSubscriptionStore : ISubscriptionStore
    {
        private readonly IRedisConnectionFactory _redisConnectionFactory;
        private readonly string _connectionString;
        private readonly string _storeKeyPrefix;

        public RedisSubscriptionStore(
            IRedisConnectionFactory redisConnectionFactory,
            string connectionString, 
            string storeKeyPrefix)
        {
            _redisConnectionFactory = redisConnectionFactory;
            _connectionString = connectionString;
            _storeKeyPrefix = storeKeyPrefix;
        }

       public Task<IEnumerable<Subscription>> GetAll()
        {
            var set = GetAllSubscriptionsKey();
            return GetSubscriptionsInSet(set);
        }

        public Task<IEnumerable<Subscription>> GetByTopic(string topic)
        {
            var set = GetTopicKey(topic);
            return GetSubscriptionsInSet(set);
        }

        public Task<IEnumerable<Subscription>> GetByEndpoint(string endpoint)
        {
            var set = GetEndpointKey(endpoint);
            return GetSubscriptionsInSet(set);
        }

        private async Task<IEnumerable<Subscription>> GetSubscriptionsInSet(string set)
        {
            var redis = GetConnection().GetDatabase();
            var allSubscriptions = await redis.SetMembersAsync(set);

            var subscriptions = allSubscriptions
                .Select(subscriptionId => Subscription.FromId(subscriptionId))
                .ToArray();
            
            return subscriptions;
        }

        public Task<bool> AddSubscription(Subscription subscription)
        {
            var redis = GetConnection().GetDatabase();

            var subscriptionKey = GetSubscriptionKey(subscription); // to detect concurrent updates

            var topicKey = GetTopicKey(subscription.Topic);
            var endpointKey = GetEndpointKey(subscription.Endpoint);
            var allSubscriptionsKey = GetAllSubscriptionsKey();
            
            var transaction = redis.CreateTransaction();

            // uses subscriptionId key for optimistic concurrency
            // if another client happens to adding this key concurrently, only the first one will publish changes
            transaction.AddCondition(Condition.KeyNotExists(subscriptionKey));

            transaction.SetAddAsync(topicKey, subscription.Id);
            transaction.SetAddAsync(endpointKey, subscription.Id);
            transaction.SetAddAsync(allSubscriptionsKey, subscription.Id);
            
            // don't really need to have a value here, it's just for optimistic concurrency, it either exists or not
            transaction.StringSetAsync(subscriptionKey, string.Empty);

            return transaction.ExecuteAsync();
        }

        public Task<bool> RemoveSubscription(Subscription subscription)
        {
            var redis = GetConnection().GetDatabase();

            // to detect concurrent updates
            var subscriptionKey = GetSubscriptionKey(subscription); 

            var topicKey = GetTopicKey(subscription.Topic);
            var endpointKey = GetEndpointKey(subscription.Endpoint);
            var allSubscriptionsKey = GetAllSubscriptionsKey();

            var transaction = redis.CreateTransaction();

            // uses subscriptionId key for optimistic concurrency
            // if another client happens to be removing this key concurrently, only the first one will publish changes
            transaction.AddCondition(Condition.KeyExists(subscriptionKey));

            transaction.SetRemoveAsync(topicKey, subscription.Id);
            transaction.SetRemoveAsync(endpointKey, subscription.Id);
            transaction.SetRemoveAsync(allSubscriptionsKey, subscription.Id);
            transaction.KeyDeleteAsync(subscriptionKey);

            return transaction.ExecuteAsync();
        }

        private string GetSubscriptionKey(Subscription subscription)
        {
            return $"{_storeKeyPrefix}.subscriptions.{subscription.Id}";
        }

        private string GetTopicKey(string topic)
        {
            return $"{_storeKeyPrefix}.subscriptions.topic.{topic}";
        }

        private string GetEndpointKey(string endpoint)
        {
            return $"{_storeKeyPrefix}.subscriptions.endpoint.{endpoint}";
        }

        private string GetAllSubscriptionsKey()
        {
            return $"{_storeKeyPrefix}.subscriptions";
        }
        
        private ConnectionMultiplexer GetConnection() => _redisConnectionFactory.GetConnection(_connectionString);
    }
}