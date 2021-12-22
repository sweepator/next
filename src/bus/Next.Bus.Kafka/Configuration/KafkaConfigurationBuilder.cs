using System;
using Next.Bus.Kafka.Transport;

namespace Next.Bus.Kafka.Configuration
{
    internal class KafkaConfigurationBuilder : IKafkaConfigurationBuilder
    {
        private readonly KafkaConfiguration _kafkaConfiguration;

        internal KafkaConfigurationBuilder(KafkaConfiguration kafkaConfiguration)
        {
            _kafkaConfiguration = kafkaConfiguration;
        }

        public IKafkaConfigurationBuilder WithBootstrapServers(string bootstrapServers)
        {
            _kafkaConfiguration.BootstrapServers = bootstrapServers;
            return this;
        }

        public IKafkaConfigurationBuilder WithConsumerGroupId(string groupId)
        {
            _kafkaConfiguration.ConsumerGroupId = groupId;
            return this;
        }

        public IKafkaConfigurationBuilder WithConsumerClientId(string clientId)
        {
            _kafkaConfiguration.ConsumerClientId = clientId;
            return this;
        }

        public IKafkaConfigurationBuilder WithConsumerTimeout(TimeSpan timeSpan)
        {
            _kafkaConfiguration.ConsumerTimeout = timeSpan;
            return this;
        }

        public IKafkaConfigurationBuilder WithErrorCheckTimeout(TimeSpan timeSpan)
        {
            _kafkaConfiguration.ErrorConsumerTimeout = timeSpan;
            return this;
        }

        public IKafkaConfigurationBuilder WithErrorCheckLock(TimeSpan timeSpan)
        {
            _kafkaConfiguration.ErrorCheckLock = timeSpan;
            return this;
        }

        public IKafkaConfigurationBuilder WithProducerClientId(string clientId)
        {
            _kafkaConfiguration.ProducerClientId = clientId;
            return this;
        }
    }
}