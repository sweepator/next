using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Next.Cqrs.Queries;
using Next.Cqrs.Queries.Projections;

namespace Next.Cqrs.Configuration
{
    public interface IProjectionsBuilder
    {
        IServiceCollection Services { get; }
        
        IDictionary<Type, IEnumerable<Type>> QueryRequestByProjectionTypes { get; }
        
        /*IProjectionsBuilder UseQueryStoreFor<TProjectionStore, TProjectionModel>()
            where TProjectionStore : class, IQueryStore<TProjectionModel>
            where TProjectionModel : class, IProjectionModel;*/

        IProjectionsBuilder QueryStoreFor<TQueryStoreService, TQueryStoreImplementation, TProjectionModel, TProjectionModelLocator>(
            bool isSync = false,
            Action<QueryStorePopulatorStartupOptions<TProjectionModel>> setup = null)
            where TProjectionModel : class, IProjectionModel
            where TQueryStoreService : class, IQueryStore<TProjectionModel>
            where TQueryStoreImplementation : class, TQueryStoreService
            where TProjectionModelLocator : class, IProjectionModelLocator;

        IProjectionsBuilder QueryStoreFor<TQueryStoreService, TQueryStoreImplementation, TProjectionModel>(
            bool isSync = false,
            Action<QueryStorePopulatorStartupOptions<TProjectionModel>> setup = null)
            where TProjectionModel : class, IProjectionModel
            where TQueryStoreService : class, IQueryStore<TProjectionModel>
            where TQueryStoreImplementation : class, TQueryStoreService;

        IProjectionsBuilder InMemoryQueryStoreFor<TProjectionModel>(Action<QueryStorePopulatorStartupOptions<TProjectionModel>> setup = null)
            where TProjectionModel : class, IProjectionModel;
        
        IProjectionsBuilder InMemoryQueryStoreFor<TProjectionModel, TProjectionModelLocator>(Action<QueryStorePopulatorStartupOptions<TProjectionModel>> setup = null)
            where TProjectionModel : class, IProjectionModel
            where TProjectionModelLocator : class, IProjectionModelLocator;

        IProjectionsBuilder AddQueryService<TProjectionModel>(Type queryServiceType)
            where TProjectionModel : class, IProjectionModel;

        IProjectionsBuilder PopulatorOptions(Action<QueryStorePopulatorOptions> setup);
    }
}