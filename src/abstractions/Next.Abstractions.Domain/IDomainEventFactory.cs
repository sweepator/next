using System;

namespace Next.Abstractions.Domain
{
    public interface IDomainEventFactory
    {
        IDomainEvent Create(
            IAggregateEvent aggregateEvent,
            IMetadata metadata,
            string aggregateIdentity,
            int version,
            DateTime timeStamp);

        IDomainEvent<TAggregate, TIdentity> Create<TAggregate, TIdentity>(
            IAggregateEvent aggregateEvent,
            IMetadata metadata,
            TIdentity id,
            int version,
            DateTime timeStamp)
            where TAggregate : IAggregateRoot<TIdentity>
            where TIdentity : IIdentity;

        IDomainEvent<TAggregate, TIdentity> Upgrade<TAggregate, TIdentity>(
            IDomainEvent domainEvent,
            IAggregateEvent aggregateEvent)
            where TAggregate : IAggregateRoot<TIdentity>
            where TIdentity : IIdentity;

        Type GetIdentityType(Type domainEventType);

        Type GetDomainEventType(Type aggregateEventType);
    }
}