using System;

namespace Next.Abstractions.Domain
{
    public interface ISerializedEvent
    {
        string AggregateId { get; }
        string AggregateName { get; }
        string EventName { get; }
        int Version { get; }
        public DateTime Timestamp { get; }
        public string Data { get; }
        public string Metadata { get; }
        public Guid? TransactionId { get; }
        public bool Commited { get; }
    }
}