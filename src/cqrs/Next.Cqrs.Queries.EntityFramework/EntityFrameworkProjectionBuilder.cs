using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Next.Cqrs.Configuration;
using Next.Cqrs.Queries.Projections;

namespace Next.Cqrs.Queries.EntityFramework
{
    internal class EntityFrameworkProjectionBuilder<TDbContext> : IEntityFrameworkProjectionBuilder
        where TDbContext: DbContext
    {
        private IProjectionsBuilder ProjectionsBuilder { get; }

        internal EntityFrameworkProjectionBuilder(IProjectionsBuilder projectionsBuilder)
        {
            ProjectionsBuilder = projectionsBuilder;
        }
        
        public IEntityFrameworkProjectionBuilder Projection<TProjectionModel>(Action<QueryStorePopulatorStartupOptions<TProjectionModel>> setup = null) 
            where TProjectionModel : class, IProjectionModel
        {
            ProjectionsBuilder
                .Services
                .TryAddSingleton<IProjectionModelFactory<TProjectionModel> , ProjectionModelFactory<TProjectionModel>>();
            
            ProjectionsBuilder
                .QueryStoreFor<IEntityFrameworkQueryStore<TProjectionModel>, EntityFrameworkQueryStore<TProjectionModel, TDbContext>, TProjectionModel>(
                    false,
                    setup);

            return this;
        }
    }
}