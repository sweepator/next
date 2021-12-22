using Next.Abstractions.Mapper;
using Next.Abstractions.Domain;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Next.Abstractions.Domain.Persistence;

namespace Next.Domain.EntityFramework
{
    public class GenericAggregateRepository<TAggregateRoot, TIdentity, TState, TDbContext, TEntity> : AggregateRepository<TAggregateRoot, TIdentity, TState>
        where TAggregateRoot : IAggregateRoot<TIdentity, TState>
        where TState : class, IState, new()
        where TIdentity : IIdentity
        where TDbContext : DbContext
        where TEntity : class
    {
        protected TDbContext Context { get; }
        protected IMapper Mapper { get; }

        protected GenericAggregateRepository(
            TDbContext context,
            IMapper mapper)
        {
            Context = context;
            Mapper = mapper;
        }

        protected virtual async Task<TEntity> GetEntity(TIdentity identity)
        {
            return await Context.FindAsync<TEntity>(identity);
        }

        protected virtual TEntity Map(TState state)
        {
            return Mapper.Map<TEntity>(state);
        }

        protected virtual void Map(TState state, TEntity entity)
        {
            Mapper.Map(state, entity);
        }

        protected virtual async Task AddEntity(TState state, TEntity entity)
        {
            await Context.AddAsync(entity);
        }

        protected virtual void UpdateEntity(TState state, TEntity entity)
        {
            Context.Update(entity);
        }

        public override async Task Save(TAggregateRoot aggregateRoot)
        {
            if (!aggregateRoot.HasChanges)
            {
                // nothing to save
                return;
            }

            var entity = await GetEntity(aggregateRoot.Id);

            if (entity == null)
            {
                entity = Map(aggregateRoot.State);
                await AddEntity(aggregateRoot.State, entity);
            }
            else
            {
                Map(aggregateRoot.State, entity);
                UpdateEntity(aggregateRoot.State, entity);
            }

            await Context.SaveChangesAsync();
        }

        protected override async Task<TState> LoadState(TIdentity id)
        {
            var entity = await GetEntity(id);
            return entity == null ? default : Mapper.Map<TState>(entity);
        }
    }
}
