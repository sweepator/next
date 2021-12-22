using System.Collections.Generic;
using System.Linq;
using Next.Abstractions.Bus;
using Next.Abstractions.Bus.Transport;
using Next.Bus.Kafka.Configuration;

namespace Next.Bus.Kafka.Transport
{
    internal class EndpointTopicNamingStrategy : ITopicNamingStrategy
    {
        private readonly IEndpointDiscovery _endpointDiscovery;

        public EndpointTopicNamingStrategy(IEndpointDiscovery endpointDiscovery)
        {
            _endpointDiscovery = endpointDiscovery;
        }
        
        public string GetProducerTopic(TransportMessage transportMessage)
        {
            return $"{KafkaConfiguration.NameSpace}.{transportMessage.Headers[MessageHeaders.Endpoint]}";
        }

        public IEnumerable<string> GetConsumerTopics()
        {
            return _endpointDiscovery
                .GetEndpoints()
                .Select(o => $"{KafkaConfiguration.NameSpace}.{o}")
                .ToArray();
        }
        
        public string GetConsumerTopic(string endpoint)
        {
            return $"{KafkaConfiguration.NameSpace}.{endpoint}";
        }
    }
}