using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using CloudNative.CloudEvents.Kafka;
using CloudNative.CloudEvents.SystemTextJson;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Next.Abstractions.Domain;
using Next.Abstractions.Serialization.Json;

namespace Next.Cqrs.Integration.Kafka
{
    public class KafkaDomainPublisher<TIntegrationEvent>: IDomainIntegrationPublisher<TIntegrationEvent>
        where TIntegrationEvent:class
    {
        private readonly ILogger<KafkaDomainPublisher<TIntegrationEvent>> _logger;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly IOptions<KafkaDomainIntegrationOptions<TIntegrationEvent>> _options;
        private static readonly CloudEventFormatter Formatter = new JsonEventFormatter();

        private static readonly ConcurrentDictionary<Type, IProducer<string, byte[]>> Producers = new();

        public KafkaDomainPublisher(
            ILogger<KafkaDomainPublisher<TIntegrationEvent>> logger,
            IJsonSerializer jsonSerializer,
            IOptions<KafkaDomainIntegrationOptions<TIntegrationEvent>> options)
        {
            _logger = logger;
            _jsonSerializer = jsonSerializer;
            _options = options;
        }

        public async Task Publish(
            TIntegrationEvent integrationEvent,
            IMetadata metadata)
        {
            var cloudEvent = new CloudEvent(CloudEventsSpecVersion.Default)
            {
                Type = integrationEvent.GetType().FullName,
                Id = Guid.NewGuid().ToString(),
                Source = new Uri("/something/1234"),
                DataContentType = MediaTypeNames.Application.Json,
                Data = _jsonSerializer.Serialize(integrationEvent),
                Time = DateTimeOffset.UtcNow
            };
            
            var message = cloudEvent.ToKafkaMessage(
                ContentMode.Binary,
                Formatter);

            if (_options.Value.AutoCreateTopic)
            {
                await CreateTopicIfNotExists();
            }
            
            var producer = Producers.GetOrAdd(typeof(TIntegrationEvent),
                key =>
                {
                    var config = new ProducerConfig()
                    {
                        BootstrapServers = _options.Value.BootstrapServers
                    };

                    var builder = new ProducerBuilder<string, byte[]>(config);
                    return builder.Build();
                });

            _logger.LogDebug("Producing cloud event kafka message {CloudEvent} to {Topic}", 
                cloudEvent, 
                _options.Value.Topic);
            
            await producer.ProduceAsync(
                _options.Value.Topic,
                message);
            
            _logger.LogDebug("Produced cloud event kafka message {CloudEvent} to {Topic}", 
                cloudEvent, 
                _options.Value.Topic);
        }

        private async Task CreateTopicIfNotExists()
        {
            var topic = _options.Value.Topic;
            
            using var adminClient = new AdminClientBuilder(new AdminClientConfig
                {
                    BootstrapServers = _options.Value.BootstrapServers
                })
                .Build();
            
            var metadata = adminClient.GetMetadata(
                topic,
                TimeSpan.FromSeconds(1));

            var topicExists = metadata
                .Topics
                .Any(o => o.Topic.Equals(topic));

            if (!topicExists)
            {
                _logger.LogDebug("Creating kafka topic: {Topic}", topic);
                await adminClient.CreateTopicsAsync(new[]
                {
                    new TopicSpecification
                    {
                        Name = topic
                    }
                });
                _logger.LogDebug("Kafka topic created: {Topic}", topic);
            }
        }
    }
}