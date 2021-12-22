using System.Collections.Generic;
using Next.Abstractions.Bus.Transport;

namespace Next.Bus.MassTransit.Transport
{
    public interface ISendEndpointStrategy
    {
        string GetProducerEndpoint(TransportMessage transportMessage);
        
        IEnumerable<string> GetConsumerEndpoints();
    }
}