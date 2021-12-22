using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Next.Cqrs.Configuration;
using Next.Cqrs.Queries.Projections;
using Next.Cqrs.Queries.Services;
using Next.Data.MongoDb;

namespace Next.Cqrs.Queries.MongoDb
{
    internal class MongoDbProjectionBuilder : IMongoDbProjectionBuilder
    {
        private IProjectionsBuilder ProjectionsBuilder { get; }
        private string ConnectionString { get; }
        private string Database { get; }

        internal MongoDbProjectionBuilder(
            IProjectionsBuilder projectionsBuilder,
            string connectionString,
            string database)
        {
            ProjectionsBuilder = projectionsBuilder;
            ConnectionString = connectionString;
            Database = database;
        }
        
        private IMongoDbProjectionBuilder InternalProjectionSetup<TProjectionModel>(
            bool isSync,
            Action<QueryStorePopulatorStartupOptions<TProjectionModel>> setup = null) where TProjectionModel : class, IProjectionModel
        {
            ProjectionsBuilder
                .Services
                .Configure<MongoDbQueryStoreOptions<TProjectionModel>>(o =>
                {
                    o.ConnectionString = ConnectionString;
                    o.DatatBase = Database;
                });
            
            ProjectionsBuilder
                .Services
                .TryAddSingleton<IMongoDbClientFactory, MongoDbClientFactory>();
            ProjectionsBuilder
                .Services
                .AddMongoDbInitializerStartupTask();
            ProjectionsBuilder
                .Services
                .TryAddSingleton<IMongoDbProjectionModelDescriptionProvider, MongoDbProjectionModelDescriptionProvider>();
            ProjectionsBuilder
                .Services
                .TryAddSingleton<IProjectionModelFactory<TProjectionModel> , ProjectionModelFactory<TProjectionModel>>();
            ProjectionsBuilder
                .AddQueryService<TProjectionModel>(typeof(QueryableService<,>));

            return this;
        }

        public IMongoDbProjectionBuilder SyncProjection<TProjectionModel>(Action<QueryStorePopulatorStartupOptions<TProjectionModel>> setup = null) 
            where TProjectionModel : class, IProjectionModel
        {
            InternalProjectionSetup(
                true,
                setup);
            
            ProjectionsBuilder
                .QueryStoreFor<IQueryableQueryStore<TProjectionModel>, MongoDbQueryStore<TProjectionModel>, TProjectionModel>(
                    false,
                    setup);

            return this;
        }

        public IMongoDbProjectionBuilder SyncProjection<TProjectionModel, TProjectionModelLocator>(Action<QueryStorePopulatorStartupOptions<TProjectionModel>> setup = null) 
            where TProjectionModel : class, IProjectionModel 
            where TProjectionModelLocator : class, IProjectionModelLocator
        {
            InternalProjectionSetup(
                true,
                setup);
            
            ProjectionsBuilder
                .QueryStoreFor<IQueryableQueryStore<TProjectionModel>, MongoDbQueryStore<TProjectionModel>, TProjectionModel, TProjectionModelLocator>(
                    false,
                    setup);

            return this;
        }

        public IMongoDbProjectionBuilder AsyncProjection<TProjectionModel>(Action<QueryStorePopulatorStartupOptions<TProjectionModel>> setup = null) 
            where TProjectionModel : class, IProjectionModel
        {
            InternalProjectionSetup(
                false,
                setup);
            
            ProjectionsBuilder
                .QueryStoreFor<IQueryableQueryStore<TProjectionModel>, MongoDbQueryStore<TProjectionModel>, TProjectionModel>(
                    false,
                    setup);
            
            return this;
        }

        public IMongoDbProjectionBuilder AsyncProjection<TProjectionModel, TProjectionModelLocator>(Action<QueryStorePopulatorStartupOptions<TProjectionModel>> setup = null) 
            where TProjectionModel : class, IProjectionModel 
            where TProjectionModelLocator : class, IProjectionModelLocator
        {
            InternalProjectionSetup(
                false,
                setup);
            
            ProjectionsBuilder
                .QueryStoreFor<IQueryableQueryStore<TProjectionModel>, MongoDbQueryStore<TProjectionModel>, TProjectionModel, TProjectionModelLocator>(
                    false,
                    setup);
            
            return this;
        }
    }
}