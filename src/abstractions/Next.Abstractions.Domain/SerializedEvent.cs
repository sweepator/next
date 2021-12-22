using System;

namespace Next.Abstractions.Domain
{
    public class SerializedEvent : ISerializedEvent
    {
        public string AggregateId { get; }
        public string AggregateName { get; }
        public string EventName { get; }
        public int Version { get; }
        public DateTime Timestamp { get; }
        public string Data { get; }
        public string Metadata { get; }
        public Guid? TransactionId { get; }
        public bool Commited { get; }

        public SerializedEvent(
            string aggregateId, 
            string aggregateName, 
            string eventName, 
            int version, 
            DateTime timestamp, 
            string data, 
            string metadata,
            Guid? transactionId = null,
            bool commited = false)
        {
            AggregateId = aggregateId;
            AggregateName = aggregateName;
            EventName = eventName;
            Version = version;
            Timestamp = timestamp;
            Data = data;
            Metadata = metadata;
            TransactionId = transactionId;
            Commited = commited;
        }
    }
}