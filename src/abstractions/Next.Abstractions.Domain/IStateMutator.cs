namespace Next.Abstractions.Domain
{
    internal interface IStateMutator
    {
        void Mutate(IState state, IAggregateEvent @event);
    }
}
