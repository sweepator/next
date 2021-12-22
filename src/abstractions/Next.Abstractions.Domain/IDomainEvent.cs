using System;

namespace Next.Abstractions.Domain
{
    public interface IDomainEvent
    {
        Type AggregateType { get; }
        
        Type EventType { get; }
        
        int Version { get; }
        
        DateTime Timestamp { get; }

        IIdentity AggregateIdentity { get; }
        
        IAggregateEvent AggregateEvent { get; }

        IMetadata Metadata { get; }
    }
    
    public interface IDomainEvent<out TAggregate, out TIdentity> : IDomainEvent
        where TAggregate : IAggregateRoot<TIdentity>
        where TIdentity : IIdentity
    {
        new TIdentity AggregateIdentity { get; }
        
        IIdentity IDomainEvent.AggregateIdentity => AggregateIdentity;
    }

    public interface IDomainEvent<out TAggregate, out TIdentity, out TAggregateEvent> : IDomainEvent<TAggregate, TIdentity>
        where TAggregate : IAggregateRoot<TIdentity>
        where TIdentity : IIdentity
        where TAggregateEvent : IAggregateEvent<TAggregate>
    {
        new TAggregateEvent AggregateEvent { get; }
        
        IAggregateEvent IDomainEvent.AggregateEvent => AggregateEvent;
    }
}