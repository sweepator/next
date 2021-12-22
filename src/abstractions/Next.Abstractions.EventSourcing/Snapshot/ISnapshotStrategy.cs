using Next.Abstractions.Domain;

namespace Next.Abstractions.EventSourcing.Snapshot
{
    public interface ISnapshotStrategy
    {
        ISnapshot Create(IAggregateRoot aggregateRoot);
    }
    
    public interface ISnapshotStrategy<TAggregateRoot, out TIdentity, out TState>: ISnapshotStrategy
        where TAggregateRoot:  IAggregateRoot<TIdentity, TState>
        where TState : class, IState
        where TIdentity : IIdentity
    {
        ISnapshot<TAggregateRoot, TIdentity, TState> Create(TAggregateRoot aggregateRoot);
    }
}