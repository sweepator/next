using System.Threading.Tasks;

namespace Next.Abstractions.Domain.Persistence
{
    public interface IAggregateRepository<TAggregateRoot, in TIdentity, TState>
        where TAggregateRoot : IAggregateRoot<TIdentity, TState>
        where TIdentity : IIdentity
        where TState : IState
    {
        Task<TAggregateRoot> FindOrDefault(TIdentity identity);
        Task<TAggregateRoot> Find(TIdentity identity);
        Task Save(TAggregateRoot aggregateRoot);
    }
}
