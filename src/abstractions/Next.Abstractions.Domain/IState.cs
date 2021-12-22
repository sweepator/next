namespace Next.Abstractions.Domain
{
    public interface IState
    {
        //object Id { get; }
        
        int Version { get; }

        /// <summary>
        /// Mutates the state by applying the aggregate event. After every mutation, the Version is increased.
        /// </summary>
        /// <param name="event">event that represents a state change in the aggregate</param>
        void Mutate(IAggregateEvent @event);
    }

    /*public interface IState<out TIdentity> : IState
    {
        new TIdentity Id { get; }

        object IState.Id => Id;
    }*/
}
