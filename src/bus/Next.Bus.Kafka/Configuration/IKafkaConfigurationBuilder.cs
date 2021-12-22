using System;
using Next.Bus.Kafka.Transport;

namespace Next.Bus.Kafka.Configuration
{
    public interface IKafkaConfigurationBuilder
    {
        IKafkaConfigurationBuilder WithBootstrapServers(string bootstrapServers);
        IKafkaConfigurationBuilder WithConsumerGroupId(string groupId);
        IKafkaConfigurationBuilder WithConsumerClientId(string clientId);
        IKafkaConfigurationBuilder WithConsumerTimeout(TimeSpan timeSpan);
        IKafkaConfigurationBuilder WithErrorCheckTimeout(TimeSpan timeSpan);
        IKafkaConfigurationBuilder WithErrorCheckLock(TimeSpan timeSpan);
        IKafkaConfigurationBuilder WithProducerClientId(string clientId);
    }
}