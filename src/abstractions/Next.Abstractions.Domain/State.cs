namespace Next.Abstractions.Domain
{
    public abstract class State: IState
    {
        private readonly IStateMutator _mutator;
        
        public int Version { get; private set; }

        protected State()
        {
            _mutator = StateMutator.For(GetType());
            Version = 0;
        }

        public State(int version)
            : this()
        {
            Version = version;
        }

        public void Mutate(IAggregateEvent @event)
        {
            _mutator.Mutate(this, @event);
            Version++;
        }
    }
}
