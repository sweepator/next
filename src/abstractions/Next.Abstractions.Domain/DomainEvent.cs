using System;

namespace Next.Abstractions.Domain
{
    public class DomainEvent<TAggregate, TIdentity, TAggregateEvent> : IDomainEvent<TAggregate, TIdentity, TAggregateEvent>
        where TAggregate : IAggregateRoot<TIdentity>
        where TIdentity : IIdentity
        where TAggregateEvent : IAggregateEvent<TAggregate>
    {
        public Type AggregateType => typeof(TAggregate);
        
        public Type EventType => typeof(TAggregateEvent);
        
        public int Version { get; }
        
        public DateTime Timestamp { get; }
        
        public TAggregateEvent AggregateEvent { get; }
        
        public IMetadata Metadata { get; }

        public TIdentity AggregateIdentity { get; }
        
        public DomainEvent(
            TAggregateEvent aggregateEvent,
            IMetadata metadata,
            DateTime timestamp,
            TIdentity aggregateIdentity,
            int version)
        {
            if (timestamp == default)
            {
                throw new ArgumentNullException(nameof(timestamp));
            }

            if (aggregateIdentity == null || string.IsNullOrEmpty(aggregateIdentity.Value))
            {
                throw new ArgumentNullException(nameof(aggregateIdentity));
            }

            if (version <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(version));
            }

            AggregateEvent = aggregateEvent ?? throw new ArgumentNullException(nameof(aggregateEvent));
            Metadata = metadata;
            Timestamp = timestamp;
            AggregateIdentity = aggregateIdentity;
            Version = version;
        }
    }
}