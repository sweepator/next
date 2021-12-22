namespace Next.Abstractions.Domain
{
    public interface IAggregateFactory<TAggregate, TIdentity, TState>
        where TAggregate : IAggregateRoot<TIdentity, TState>
        where TState : class, IState, new()
        where TIdentity : IIdentity
    {
        TAggregate CreateNew(TIdentity id);

        TAggregate CreateFromEvents(
            TIdentity id, 
            IAggregateEvent[] events);

        TAggregate CreateFromState(TIdentity id, TState state);

        TAggregate Create(
            TIdentity id,
            TState state = null,
            IAggregateEvent[] events = null);
    }
}
