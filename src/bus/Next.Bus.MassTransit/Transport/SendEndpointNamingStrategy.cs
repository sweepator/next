using System.Collections.Generic;
using System.Linq;
using Next.Abstractions.Bus;
using Next.Abstractions.Bus.Transport;
using Next.Bus.MassTransit.Configuration;

namespace Next.Bus.MassTransit.Transport
{
    internal class SendEndpointNamingStrategy : ISendEndpointStrategy
    {
        private readonly IEndpointDiscovery _endpointDiscovery;

        public SendEndpointNamingStrategy(IEndpointDiscovery endpointDiscovery)
        {
            _endpointDiscovery = endpointDiscovery;
        }
            
        public string GetProducerEndpoint(TransportMessage transportMessage)
        {
            return $"{MassTransitBusConfiguration.NameSpace}.{transportMessage.Headers[MessageHeaders.Endpoint]}";
        }

        public IEnumerable<string> GetConsumerEndpoints()
        {
            return _endpointDiscovery
                .GetEndpoints()
                .Select(o => $"{MassTransitBusConfiguration.NameSpace}.{o}")
                .ToArray();
        }
    }
}