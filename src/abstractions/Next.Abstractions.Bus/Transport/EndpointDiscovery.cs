using System;
using System.Collections.Generic;

namespace Next.Abstractions.Bus.Transport
{
    internal class EndpointDiscovery : IEndpointDiscovery
    {
        private readonly List<string> _endpoints = new();
        
        public IEnumerable<string> GetEndpoints()
        {
            return _endpoints;
        }

        public void RegisterEndpoint(string endpoint)
        {
            if (_endpoints.Contains(endpoint))
            {
                throw new ApplicationException($"Endpoint already exists: {endpoint}");
            }
            
            _endpoints.Add(endpoint);
        }
    }
}