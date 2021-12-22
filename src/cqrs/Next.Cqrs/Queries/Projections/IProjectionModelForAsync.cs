using System.Threading;
using System.Threading.Tasks;
using Next.Abstractions.Domain;

namespace Next.Cqrs.Queries.Projections
{
    public interface IProjectionModelForAsync<in TAggregate, in TIdentity, in TAggregateEvent>
        where TAggregate : IAggregateRoot<TIdentity>
        where TIdentity : IIdentity
        where TAggregateEvent : IAggregateEvent<TAggregate>
    {
        Task Apply(
            IProjectionModelContext context, 
            IDomainEvent<TAggregate, TIdentity, TAggregateEvent> domainEvent,
            CancellationToken cancellationToken = default);
    }
}