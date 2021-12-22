using System;

namespace Next.Abstractions.EventSourcing.Snapshot
{
    public interface ISerializedSnapshot
    {
        string AggregateId { get; }
        string AggregateName { get; }
        string Name { get; }
        int Version { get; }
        public DateTime Timestamp { get; }
        public string Data { get; }
        public string Metadata { get; }
    }
}