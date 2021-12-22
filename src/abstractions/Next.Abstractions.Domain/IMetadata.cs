using System;
using Next.Core.Metadata;

namespace Next.Abstractions.Domain
{
    public interface IMetadata : IMetadataContainer
    {
        //IEventId EventId { get; }
        //ISourceId SourceId { get; }
        int SchemaVersion { get; }
        Guid TransactionId { get; set; }
        bool Commited { get; set; }
        //DateTimeOffset Timestamp { get; }
        //long TimestampEpoch { get; }
        //int AggregateSequenceNumber { get; }
        //string AggregateId { get; }
        //string AggregateName { get; }

        //IMetadata CloneWith(params KeyValuePair<string, string>[] keyValuePairs);
        //IMetadata CloneWith(IEnumerable<KeyValuePair<string, string>> keyValuePairs);
    }
}