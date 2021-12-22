using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Next.Abstractions.Bus;
using Next.Abstractions.Bus.Transport;
using Next.Abstractions.Serialization.Json;
using Next.Data.Redis;
using StackExchange.Redis;

namespace Next.Bus.Redis.Transport
{
    public class RedisTransport : IInboundTransport, IOutboundTransport
    {
        private readonly IJsonSerializer _jsonSerializer;
        private readonly string _connectionString;
        private readonly string _inputQueue;
        private readonly string _keyFormat;
        private readonly IRedisConnectionFactory _redisConnectionFactory;

        public RedisTransport(
            IRedisConnectionFactory redisConnectionFactory,
            IJsonSerializer jsonSerializer,
            string connectionString, 
            string prefix, 
            string inputQueue)
        {
            _redisConnectionFactory = redisConnectionFactory;
            _jsonSerializer = jsonSerializer;
            _connectionString = connectionString;
            _inputQueue = inputQueue;

            _keyFormat = !string.IsNullOrWhiteSpace(prefix) ? "{0}" : $"{prefix}.{{0}}";
        }

        private ConnectionMultiplexer GetConnection() => _redisConnectionFactory.GetConnection(_connectionString);

        private string GetMessageKey(string queue, string messageId)
        {
            var key = $"{queue}:msg:{messageId}";
            return GetRedisKey(key);
        }

        private string GetMessageRetryCountKey(string queue, string messageId)
        {
            var key = $"{queue}:msg:{messageId}:retry";
            return GetRedisKey(key);
        }

        private string GetRedisKey(string key)
        {
            return string.Format(_keyFormat, key);
        }

        public Task Send(TransportMessage message)
        {
            return SendMultiple(new [] { message });
        }

        public Task SendMultiple(IEnumerable<TransportMessage> messages)
        {
            var redis = GetConnection().GetDatabase();

            var tr = redis.CreateTransaction();

            foreach (var message in messages)
            {
                var jsonMsg = _jsonSerializer.Serialize(message);
                var outputQueue = message.Headers[MessageHeaders.Endpoint];

                var messageKey = GetMessageKey(outputQueue, message.Id);
                var outputQueueKey = GetRedisKey(outputQueue);

                tr.StringSetAsync(messageKey, jsonMsg, TimeSpan.FromDays(7), When.NotExists);
                tr.ListLeftPushAsync(outputQueueKey, message.Id);
            }

            return tr.ExecuteAsync();
        }

        public async Task<IMessageTransaction> Receive(TimeSpan timeout)
        {
            var transactionId = Guid.NewGuid().ToString();

            var messageId = await GetQueuedMessageId(transactionId, timeout);

            if (messageId != null)
            {
                var message = await GetMessage(messageId);
                if (message != null)
                {
                    var deliveryCount = message.Item2 + 1; // retryCount + 1
                    var transaction = new RedisMessageTransaction(
                        message.Item1, 
                        deliveryCount,
                        async () => await CommitTransaction(transactionId, messageId),
                        async () => await FailTransaction(transactionId, messageId));

                    return transaction;
                }
            }

            return null;
        }

        private async Task<string> GetQueuedMessageId(
            string transactionId, 
            TimeSpan timeout)
        {
            var redis = GetConnection().GetDatabase();

            var inputQueueKey = GetRedisKey(_inputQueue);
            var transactionIdKey = GetRedisKey(transactionId);

            string id = await redis.ListRightPopLeftPushAsync(
                inputQueueKey, 
                transactionIdKey);

            var sw = Stopwatch.StartNew();
            while (id == null && sw.Elapsed < timeout)
            {
                await Task.Delay(50);
                id = await redis.ListRightPopLeftPushAsync(
                    inputQueueKey, 
                    transactionIdKey);
            }

            return id;
        }

        private async Task<Tuple<TransportMessage, int>> GetMessage(string messageId)
        {
            var redis = GetConnection().GetDatabase();

            var messageKey = GetMessageKey(
                _inputQueue, 
                messageId);
            var retryCountKey = GetMessageRetryCountKey(
                _inputQueue,
                messageId);

            var data = await redis.StringGetAsync(new RedisKey[]
            {
                messageKey, 
                retryCountKey
            });

            if(data[0].IsNullOrEmpty)
            {
                return null;
            }
            
            string jsonMsg = data[0];
            var retryCount = data[1].IsNull ? 0 : int.Parse(data[1]);

            var message = _jsonSerializer.Deserialize<TransportMessage>(jsonMsg);
            return new Tuple<TransportMessage, int>(message, retryCount);
        }

        private Task CommitTransaction(string transactionId, string messageId)
        {
            var redis = GetConnection().GetDatabase();

            var tr = redis.CreateTransaction();

            var messageKey = GetMessageKey(
                _inputQueue, 
                messageId);
            var messageRetryCountKey = GetMessageRetryCountKey(
                _inputQueue, 
                messageId);
            var transactionIdKey = GetRedisKey(transactionId);

            tr.KeyDeleteAsync(messageKey);
            tr.KeyDeleteAsync(messageRetryCountKey); 
            tr.KeyDeleteAsync(transactionIdKey);
            
            return tr.ExecuteAsync();
        }

        private Task FailTransaction(
            string transactionId, 
            string messageId)
        {
            var redis = GetConnection().GetDatabase();

            var tr = redis.CreateTransaction();

            var messageRetryCountKey = GetMessageRetryCountKey(
                _inputQueue, 
                messageId);
            var transactionIdKey = GetRedisKey(transactionId);
            var inputQueueKey = GetRedisKey(_inputQueue);

            tr.StringIncrementAsync(messageRetryCountKey);
            tr.ListRightPopLeftPushAsync(
                transactionIdKey, 
                inputQueueKey);

            return tr.ExecuteAsync();
        }
    }
}