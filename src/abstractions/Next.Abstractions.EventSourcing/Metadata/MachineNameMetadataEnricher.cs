using System;
using System.Collections.Generic;
using Next.Abstractions.Domain;

namespace Next.Abstractions.EventSourcing.Metadata
{
    public class MachineNameMetadataEnricher : IMetadataEnricher
    {
        private static readonly KeyValuePair<string, string> Metadata; 

        static MachineNameMetadataEnricher()
        {
            Metadata = new KeyValuePair<string, string>("EnvironmentMachineName", Environment.MachineName);
        }
        
        public void Enrich(IDomainEvent domainEvent)
        {
            domainEvent.Metadata.Add(Metadata);
        }
    }
}