using System.Collections.Generic;

namespace Next.Abstractions.Bus.Transport
{
    public interface IEndpointDiscovery
    {
        IEnumerable<string> GetEndpoints();
    }
}