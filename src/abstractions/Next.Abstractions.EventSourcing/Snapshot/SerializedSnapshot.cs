using System;

namespace Next.Abstractions.EventSourcing.Snapshot
{
    public class SerializedSnapshot : ISerializedSnapshot
    {
        public string AggregateId { get; }
        public string AggregateName { get; }
        public string Name { get; }
        public int Version { get; }
        public DateTime Timestamp { get; }
        public string Data { get; }
        public string Metadata { get; }

        public SerializedSnapshot(
            string aggregateId, 
            string aggregateName, 
            string name, 
            int version, 
            DateTime timestamp, 
            string data, 
            string metadata)
        {
            AggregateId = aggregateId;
            AggregateName = aggregateName;
            Name = name;
            Version = version;
            Timestamp = timestamp;
            Data = data;
            Metadata = metadata;
        }
    }
}