using Microsoft.Extensions.Logging;
using Next.Abstractions.Bus.Transport;
using Next.Abstractions.Serialization.Json;
using Next.Bus.Kafka.Configuration;

namespace Next.Bus.Kafka.Transport
{
    public class KafkaTransportFactory : BrokerlessTransportFactory
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly KafkaConfiguration _configuration;
        private readonly ITopicNamingStrategy _topicNamingStrategy;
        private readonly IJsonSerializer _jsonSerializer;

        public KafkaTransportFactory(
            ILoggerFactory loggerFactory,
            KafkaConfiguration configuration,
            ITopicNamingStrategy topicNamingStrategy,
            IJsonSerializer jsonSerializer)
        {
            _loggerFactory = loggerFactory;
            _configuration = configuration;
            _topicNamingStrategy = topicNamingStrategy;
            _jsonSerializer = jsonSerializer;
        }

        protected override IInboundTransport CreateInboundTransport(string endpoint)
        {
            return new KafkaConsumerTransport(
                _loggerFactory.CreateLogger<KafkaConsumerTransport>(),
                _topicNamingStrategy,
                _jsonSerializer,
                endpoint,
                _configuration.BootstrapServers,
                _configuration.ConsumerGroupId,
                _configuration.ConsumerClientId,
                _configuration.ConsumerTimeout,
                _configuration.ErrorCheckLock,
                _configuration.ErrorConsumerTimeout);
        }

        protected override IOutboundTransport CreateOutboundTransport()
        {
            return new KafkaProducerTransport(
                _topicNamingStrategy,
                _jsonSerializer,
                _configuration.BootstrapServers,
                _configuration.ProducerClientId);
        }
    }
}