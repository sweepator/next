using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Microsoft.Extensions.Logging;
using Next.Abstractions.Bus.Transport;
using Next.Abstractions.Serialization.Json;
using Next.Bus.Kafka.Configuration;

namespace Next.Bus.Kafka.Transport
{
    public class KafkaConsumerTransport: IInboundTransport, IDisposable
    {
        private readonly ILogger<KafkaConsumerTransport> _logger;
        private readonly ITopicNamingStrategy _topicNamingStrategy;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly string _bootstrapServers;
        private readonly string _groupId;
        private readonly string _clientId;
        private readonly TimeSpan _consumerTimeout;
        private readonly TimeSpan _errorConsumerTimeout;
        private IConsumer<string, string> _consumer;
        private IConsumer<string, string> _errorConsumer;
        private IProducer<string, string> _errorProducer;
        private readonly ConcurrentQueue<ConsumeResult<string, string>> _retryQueue = new();
        private readonly ConcurrentDictionary<string, bool> _topicCheckStatus = new();
        private readonly string _endpoint;
        private readonly Timer _timer;

        public KafkaConsumerTransport(
            ILogger<KafkaConsumerTransport> logger,
            ITopicNamingStrategy topicNamingStrategy,
            IJsonSerializer jsonSerializer,
            string endpoint,
            string bootstrapServers,
            string groupId,
            string clientId,
            TimeSpan consumerTimeout,
            TimeSpan errorCheckLock,
            TimeSpan errorConsumerTimeout)
        {
            _logger = logger;
            _topicNamingStrategy = topicNamingStrategy;
            _jsonSerializer = jsonSerializer;
            _bootstrapServers = bootstrapServers;
            _groupId = groupId;
            _clientId = clientId;
            _consumerTimeout = consumerTimeout;
            _errorConsumerTimeout = errorConsumerTimeout;
            _endpoint = endpoint;

            _timer = new Timer(
                ConsumeErrorMessages, 
                null, 
                TimeSpan.Zero, 
                errorCheckLock);
        }
        
        private void ConsumeErrorMessages(object state)
        {
            _ = ProcessErrorMessages();
        }

        private async Task ProcessErrorMessages()
        {
            await EnsureInitialization();

            var retryMessage = _errorConsumer.Consume(_errorConsumerTimeout);

            if (retryMessage != null)
            {
                _retryQueue.Enqueue(retryMessage);
            }
        }

        public async Task<IMessageTransaction> Receive(TimeSpan timeout)
        {
            await EnsureInitialization();

            // if retry queue has no messages, then we try to get a messages from original consumer
            if (!_retryQueue.TryDequeue(out var consumeResult))
            {
                consumeResult = _consumer.Consume(timeout);
            }

            if (consumeResult is {IsPartitionEOF: false})
            {
                var transportMessage = CreateTransportMessage(consumeResult.Message);

                var hasDeliveryCount = consumeResult.Message.Headers.Any(o => o.Key.Equals(KafkaMessageHeaders.DeliveryCount));
                var deliveryCount =
                    hasDeliveryCount
                        ? int.Parse(Encoding.UTF8
                            .GetString(consumeResult.Message.Headers
                                .Single(o => o.Key.Equals(KafkaMessageHeaders.DeliveryCount))
                                .GetValueBytes()))
                        : 0;
                    
                var transaction = new KafkaMessageTransaction(
                    transportMessage,
                    deliveryCount,
                    async () => await CommitTransaction(consumeResult),
                    async () => await FailTransaction(
                        deliveryCount,
                        transportMessage,
                        consumeResult));

                return transaction;
            }
                
            return null;
        }

        private IConsumer<string, string> InitializeConsumer()
        {
            var config = new ConsumerConfig
            {
                BootstrapServers = _bootstrapServers,
                EnableAutoCommit = false,
                EnableAutoOffsetStore = false,
                GroupId = GetGroupId(),
                AutoOffsetReset = AutoOffsetReset.Earliest,
                ClientId = GetClientId(),
            };

            var consumer = new ConsumerBuilder<string, string>(config)
                .Build();

            consumer.Subscribe(GetTopics());
            return consumer;
        }
        
        private IConsumer<string, string> InitializeErrorConsumer()
        {
            var config = new ConsumerConfig
            {
                BootstrapServers = _bootstrapServers,
                EnableAutoCommit = false,
                EnableAutoOffsetStore = false,
                GroupId = GetGroupId(),
                AutoOffsetReset = AutoOffsetReset.Earliest,
                ClientId = GetClientId(),
            };

            var consumer = new ConsumerBuilder<string, string>(config)
                .Build();

            var errorTopics = GetTopics().Select(GetErrorTopic);
            consumer.Subscribe(errorTopics);
            return consumer;
        }
        
        private IProducer<string, string> InitializeErrorProducer()
        {
            var config = new ProducerConfig()
            {
                BootstrapServers = _bootstrapServers,
                ClientId =  GetClientId(),
            };

            return new ProducerBuilder<string, string>(config)
                .Build();
        }
        
        private IEnumerable<string> GetTopics()
        {
            return _topicNamingStrategy.GetConsumerTopics();
        }
        
        private async Task EnsureInitialization()
        {
            await EnsureTopics();
            
            _consumer ??= InitializeConsumer();
            _errorProducer ??= InitializeErrorProducer();
            _errorConsumer ??= InitializeErrorConsumer();
        }
        
        private Task CommitTransaction(ConsumeResult<string, string> consumeResult)
        {
            var consumer = GetConsumerByTopic(consumeResult.Topic);
                
            consumer.StoreOffset(consumeResult);
            consumer.Commit(consumeResult);
            
            return Task.CompletedTask;
        }
    
        private IConsumer<string,string> GetConsumerByTopic(string topic)
        {
            return _errorConsumer.Subscription.Contains(topic) ? 
                _errorConsumer : 
                _consumer;
        }

        private static string GetErrorTopic(string endpoint)
        {
            var topic = $"{endpoint}.error";
            return topic;
        }
        
        private string GetGroupId()
        {
            return _groupId ?? $"{_endpoint}-group";
        }
        
        private string GetClientId()
        {
            return _clientId ?? $"{KafkaConfiguration.NameSpace}-{Dns.GetHostName().ToLower()}-consumer";
        }

        private async Task FailTransaction(
            int deliveryCount,
            TransportMessage transportMessage,
            ConsumeResult<string, string> consumeResult)
        {
            var consumer = GetConsumerByTopic(consumeResult.Topic);
            var errorTopic = deliveryCount == 0 ? 
                GetErrorTopic(_topicNamingStrategy.GetConsumerTopic(_endpoint)) : 
                consumeResult.Topic;

            consumer.StoreOffset(consumeResult);
            consumer.Commit(consumeResult);

            deliveryCount++;
            var kafkaMessage = CreateKafkaMessage(
                transportMessage,
                deliveryCount);

            await _errorProducer.ProduceAsync(
                errorTopic,
                kafkaMessage);
        }
        
        private TransportMessage CreateTransportMessage(Message<string, string> message)
        {
            return _jsonSerializer.Deserialize<TransportMessage>(message.Value);
        }
        
        private Message<string, string> CreateKafkaMessage(
            TransportMessage message,
            int deliveryCount)
        {
            var headers = new Headers();

            foreach (var (key, value) in message.Headers)
            {
                headers.Add(key, Encoding.UTF8.GetBytes(value));
            }
            
            headers.Add(KafkaMessageHeaders.DeliveryCount, Encoding.UTF8.GetBytes(deliveryCount.ToString()));

            return new Message<string, string>
            {
                Key = message.Id,
                Headers = headers,
                Value = _jsonSerializer.Serialize(message)
            };
        }
        
        private bool CheckIfTopicExists(string topic)
        {
            using var adminClient = new AdminClientBuilder(new AdminClientConfig
                {
                    BootstrapServers = _bootstrapServers
                })
                .Build();
            
            var metadata = adminClient.GetMetadata(
                topic,
                TimeSpan.FromSeconds(1));

            var topicExists = metadata
                .Topics
                .Any(o => o.Topic.Equals(topic));

            return topicExists;
        }

        private async Task<bool> GetOrCreateTopic(string topic)
        {
            var topicExists = CheckIfTopicExists(topic);

            if (topicExists)
            {
                _logger.LogDebug("Kafka topic already exists {Topic}", topic);
                return true;
            }
            
            _logger.LogDebug("Creating kafka topic: {Topic}", topic);
            using var adminClient = new AdminClientBuilder(new AdminClientConfig
                {
                    BootstrapServers = _bootstrapServers
                })
                .Build();

            await adminClient.CreateTopicsAsync(new[]
            {
                new TopicSpecification
                {
                    Name = topic
                }
            });

            _logger.LogDebug("Kafka topic created: {Topic}", topic);
            
            return true;
        }

        private async Task<bool> EnsureTopics()
        {
            if (!_topicCheckStatus.IsEmpty &&
                _topicCheckStatus.All(o => o.Value))
            {
                return true;
            }
            
            var topics = GetTopics().ToList();
            topics.AddRange(GetTopics().Select(GetErrorTopic));

            foreach (var topic in topics)
            {
                var state = await GetOrCreateTopic(topic);
                _topicCheckStatus.AddOrUpdate(topic,
                    key => state,
                    (k, oldValue) => state);
            }

            return _topicCheckStatus.All(o => o.Value);
        }

        
        public void Dispose()
        {
            _consumer?.Close();
            _consumer?.Dispose();
            
            _errorConsumer?.Close();
            _errorConsumer?.Dispose();
            
            _errorProducer?.Dispose();
        }
    }
}