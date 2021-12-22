using System.Threading.Tasks;
using System.Collections.Generic;
using Next.Abstractions.Data;
using Next.Abstractions.Domain;
using Next.Abstractions.EventSourcing.Snapshot;

namespace Next.Abstractions.EventSourcing
{
    public interface IEventStore
    {
        Task<IEnumerable<IDomainEvent>> Load<TAggregateRoot, TIdentity>(TIdentity id)
            where TAggregateRoot : IAggregateRoot<TIdentity>
            where TIdentity : IIdentity;

        Task<IEnumerable<IDomainEvent>> LoadRange<TAggregateRoot, TIdentity>(
            TIdentity id,
            int start,
            int? end = null)
            where TAggregateRoot : IAggregateRoot<TIdentity>
            where TIdentity : IIdentity;

        Task<IPagedList<IDomainEvent>> LoadAllEvents(PageSelection pageSelection);

        Task Append<TAggregateRoot, TIdentity>(
            TIdentity id,
            int expectedVersion,
            IEnumerable<IDomainEvent> events)
            where TAggregateRoot : IAggregateRoot<TIdentity>
            where TIdentity : IIdentity;
        
        Task<Snapshot<TAggregateRoot, TIdentity, TState>> GetLastSnapshot<TAggregateRoot, TState, TIdentity>(TIdentity id)
            where TAggregateRoot : IAggregateRoot<TIdentity, TState>
            where TState : class, IState
            where TIdentity : IIdentity;

        Task AddSnapshot(ISnapshot snapshot);
    }
}