using Next.Abstractions.Domain;

namespace Next.Cqrs.Queries.Projections
{
    public interface IProjectionModelFor<in TAggregate, in TIdentity, in TAggregateEvent>
        where TAggregate : IAggregateRoot<TIdentity>
        where TIdentity : IIdentity
        where TAggregateEvent : IAggregateEvent<TAggregate>
    {
        void Apply(
            IProjectionModelContext context, 
            IDomainEvent<TAggregate, TIdentity, TAggregateEvent> domainEvent);
    }
}