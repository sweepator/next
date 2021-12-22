using System.Threading.Tasks;

namespace Next.Abstractions.Domain.Persistence
{
    public abstract class AggregateRepository<TAggregateRoot, TIdentity, TState> : IAggregateRepository<TAggregateRoot, TIdentity, TState>
        where TAggregateRoot : IAggregateRoot<TIdentity, TState>
        where TState : class, IState, new()
        where TIdentity : IIdentity
    {
        private static readonly IAggregateFactory<TAggregateRoot, TIdentity, TState> Factory = AggregateFactory
                .For<TAggregateRoot, TIdentity, TState>();

        public async Task<TAggregateRoot> Find(TIdentity identity)
        {
            var state = await LoadState(identity);

            if (state == null)
            {
                return default;
            }

            var aggregate = Factory.CreateFromState(identity, state);

            return aggregate;
        }

        public async Task<TAggregateRoot> FindOrDefault(TIdentity identity)
        {
            var state = await LoadState(identity);

            var aggregate = state == null ?
                Factory.CreateNew(identity) :
                Factory.CreateFromState(identity, state);

            return aggregate;
        }

        public abstract Task Save(TAggregateRoot aggregateRoot);

        protected abstract Task<TState> LoadState(TIdentity id);
    }
}
