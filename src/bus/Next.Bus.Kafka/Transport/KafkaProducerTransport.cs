using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Confluent.Kafka;
using Next.Abstractions.Bus.Transport;
using Next.Abstractions.Serialization.Json;
using Next.Bus.Kafka.Configuration;

namespace Next.Bus.Kafka.Transport
{
    public class KafkaProducerTransport: IOutboundTransport
    {
        private readonly ITopicNamingStrategy _topicNamingStrategy;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly string _bootstrapServers;
        private readonly string _clientId;
        private readonly IProducer<string, string> _producer;

        public KafkaProducerTransport(
            ITopicNamingStrategy topicNamingStrategy,
            IJsonSerializer jsonSerializer,
            string bootstrapServers,
            string clientId)
        {
            _topicNamingStrategy = topicNamingStrategy;
            _jsonSerializer = jsonSerializer;
            _bootstrapServers = bootstrapServers;
            _clientId = clientId;
            _producer = InitializeProducer();
        }
        
        public async Task Send(TransportMessage message)
        {
            var topic = _topicNamingStrategy.GetProducerTopic(message);

            var kafkaMessage = CreateKafkaMessage(message);
            await _producer.ProduceAsync(
                topic,
                kafkaMessage);
        }

        public async Task SendMultiple(IEnumerable<TransportMessage> messages)
        {
            foreach (var message in messages)
            {
                await Send(message);
            }
        }

        private IProducer<string, string> InitializeProducer()
        {
            var config = new ProducerConfig()
            {
                BootstrapServers = _bootstrapServers,
                ClientId = GetClientId(),
            };

            var builder = new ProducerBuilder<string, string>(config);
            return builder.Build();
        }
        
        private string GetClientId()
        {
            return _clientId ?? $"{KafkaConfiguration.NameSpace}-{Dns.GetHostName().ToLower()}-producer";
        }

        private Message<string, string> CreateKafkaMessage(TransportMessage message)
        {
            var headers = new Headers();

            foreach (var (key, value) in message.Headers)
            {
                headers.Add(key, Encoding.UTF8.GetBytes(value));
            }

            return new Message<string, string>
            {
                Key = message.Id,
                Headers = headers,
                Value = _jsonSerializer.Serialize(message)
            };
        }
    }
}