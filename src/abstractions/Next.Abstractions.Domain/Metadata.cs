using System;
using System.Collections.Generic;
using System.Linq;
using Next.Core.Metadata;

namespace Next.Abstractions.Domain
{
    public class Metadata : MetadataContainer, IMetadata
    {
        public int SchemaVersion 
        {
            get => GetMetadataValue(nameof(SchemaVersion), int.Parse);
            set => this[nameof(SchemaVersion)] = value.ToString();
        }
        
        public Guid TransactionId 
        {
            get => GetMetadataValue(nameof(TransactionId), Guid.Parse);
            set => this[nameof(TransactionId)] = value.ToString();
        }
        
        public bool Commited
        {
            get => GetMetadataValue(nameof(Commited), bool.Parse);
            set => this[nameof(Commited)] = value.ToString();
        }

        public Metadata()
        {
        }
        
        public Metadata(IEnumerable<KeyValuePair<string, string>> keyValuePairs)
            : base(keyValuePairs.ToDictionary(kv => kv.Key, kv => kv.Value))
        {
        }
        
    }
}