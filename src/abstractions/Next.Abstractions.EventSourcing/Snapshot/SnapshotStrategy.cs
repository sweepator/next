using Next.Abstractions.Domain;

namespace Next.Abstractions.EventSourcing.Snapshot
{
    public abstract class SnapshotStrategy<TAggregateRoot, TIdentity, TState>:  ISnapshotStrategy<TAggregateRoot,  TIdentity, TState>
        where TAggregateRoot:  IAggregateRoot<TIdentity, TState>
        where TState : class, IState
        where TIdentity : IIdentity
    {
        public abstract ISnapshot<TAggregateRoot, TIdentity, TState> Create(TAggregateRoot aggregateRoot);

        public ISnapshot Create(IAggregateRoot aggregateRoot)
        {
            return Create((TAggregateRoot) aggregateRoot);
        }
    }
}