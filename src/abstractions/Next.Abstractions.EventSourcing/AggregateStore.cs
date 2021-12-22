using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Next.Abstractions.Domain;
using Next.Abstractions.Domain.Persistence;
using Next.Abstractions.EventSourcing.Snapshot;

namespace Next.Abstractions.EventSourcing
{
    public class AggregateStore<TAggregateRoot, TIdentity, TState> : IAggregateRepository<TAggregateRoot, TIdentity, TState>
        where TAggregateRoot : class, IAggregateRoot<TIdentity, TState>
        where TState : class, IState, new()
        where TIdentity : IIdentity
    {
        private static readonly IAggregateFactory<TAggregateRoot, TIdentity, TState> Factory = AggregateFactory
                .For<TAggregateRoot, TIdentity, TState>();

        private readonly IEventStore _eventStore;
        private readonly ISnapshotProcessor _snapshotProcessor;
        private readonly IEnumerable<ISnapshotStrategy> _snapshotStrategies;

        public AggregateStore(
            IEventStore eventStore,
            ISnapshotProcessor snapshotProcessor,
            IEnumerable<ISnapshotStrategy> snapshotStrategies)
        {
            _eventStore = eventStore;
            _snapshotProcessor = snapshotProcessor;
            _snapshotStrategies = snapshotStrategies;
        }

        public async Task<TAggregateRoot> Find(TIdentity identity)
        {
            var snapshot = await _eventStore.GetLastSnapshot<TAggregateRoot, TState, TIdentity>(identity);

            var events = snapshot != null
                ? await _eventStore.LoadRange<TAggregateRoot, TIdentity>(
                    identity,
                    snapshot.AggregateVersion + 1)
                : await _eventStore.Load<TAggregateRoot, TIdentity>(identity);

            if (snapshot == null &&
                !events.Any())
            {
                return null;
            }

            var aggregateRoot = snapshot != null
                ? Factory.Create(
                    identity,
                    snapshot.State,
                    events.Select(e => e.AggregateEvent).ToArray())
                : Factory.CreateFromEvents(
                    identity,
                    events.Select(e => e.AggregateEvent).ToArray());
            
            return aggregateRoot;
        }

        public async Task<TAggregateRoot> FindOrDefault(TIdentity identity)
        {
            var snapshot = await _eventStore.GetLastSnapshot<TAggregateRoot, TState, TIdentity>(identity);

            var events = snapshot != null
                ? await _eventStore.LoadRange<TAggregateRoot, TIdentity>(
                    identity,
                    snapshot.AggregateVersion + 1)
                : await _eventStore.Load<TAggregateRoot, TIdentity>(identity);

            if (snapshot == null &&
                !events.Any())
            {
                return Factory.CreateNew(identity);
            }

            var aggregateRoot = snapshot != null
                ? Factory.Create(
                    identity,
                    snapshot.State,
                    events.Select(e => e.AggregateEvent).ToArray())
                : Factory.CreateFromEvents(
                    identity,
                    events.Select(e => e.AggregateEvent).ToArray());
            
            return aggregateRoot;
        }

        public async Task Save(TAggregateRoot aggregateRoot)
        {
            if (!aggregateRoot.HasChanges)
            {
                return;
            }

            await _eventStore.Append<TAggregateRoot, TIdentity>(
                aggregateRoot.Id,
                aggregateRoot.Version,
                aggregateRoot.Changes);

            var snapshotStrategy = _snapshotStrategies
                    .SingleOrDefault(o => o is ISnapshotStrategy<TAggregateRoot, TIdentity, TState>)
                as ISnapshotStrategy<TAggregateRoot, TIdentity, TState>;

            var snapshot = snapshotStrategy?.Create(aggregateRoot);

            if (snapshot != null)
            {
                _snapshotProcessor.AddSnapshot(snapshot);
            }
        }
    }
}
