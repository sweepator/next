using System;

namespace Next.Abstractions.Bus.Configuration
{
    public class AggregateEventEndpoint
    {
        public string Endpoint { get; } 
        
        public Type AggregateEventType { get; }

        public AggregateEventEndpoint(
            string endpoint, 
            Type aggregateEventType)
        {
            Endpoint = endpoint;
            AggregateEventType = aggregateEventType;
        }
    }
}