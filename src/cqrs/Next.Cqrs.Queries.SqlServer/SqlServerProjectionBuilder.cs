using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Next.Cqrs.Configuration;
using Next.Cqrs.Queries.Projections;
using Next.Cqrs.Queries.SqlServer.Services;

namespace Next.Cqrs.Queries.SqlServer
{
    internal class SqlServerProjectionBuilder : ISqlServerProjectionBuilder
    {
        private IProjectionsBuilder ProjectionsBuilder { get; }
        private string ConnectionString { get; }

        internal SqlServerProjectionBuilder(
            IProjectionsBuilder projectionsBuilder,
            string connectionString)
        {
            ProjectionsBuilder = projectionsBuilder;
            ConnectionString = connectionString;
        }

        private ISqlServerProjectionBuilder Projection<TProjectionModel>(
            bool isSync = false,
            Action<QueryStorePopulatorStartupOptions<TProjectionModel>> setup = null) 
            where TProjectionModel : class, IProjectionModel
        {
            ProjectionsBuilder
                .Services
                .Configure<SqlServerQueryStoreOptions<TProjectionModel>>(o =>
                {
                    o.ConnectionString = ConnectionString;
                });
            ProjectionsBuilder
                .AddQueryService<TProjectionModel>(typeof(SqlServerQueryService<,>));
            ProjectionsBuilder
                .Services
                .TryAddSingleton<IProjectionModelSqlGenerator, ProjectionModelSqlGenerator>();
            ProjectionsBuilder
                .Services
                .TryAddSingleton<IProjectionModelSqlMetadataProvider, ProjectionModelSqlMetadataProvider>();
            ProjectionsBuilder
                .Services
                .TryAddSingleton<IProjectionModelFactory<TProjectionModel>, ProjectionModelFactory<TProjectionModel>>();
            ProjectionsBuilder
                .QueryStoreFor<ISqlServerQueryStore<TProjectionModel>, SqlServerQueryStore<TProjectionModel>, TProjectionModel>(
                    isSync,
                    setup);

            return this;
        }

        public ISqlServerProjectionBuilder AsyncProjection<TProjectionModel>(Action<QueryStorePopulatorStartupOptions<TProjectionModel>> setup = null) 
            where TProjectionModel : class, IProjectionModel
        {
            return Projection(
                false, 
                setup);
        }

        public ISqlServerProjectionBuilder SyncProjection<TProjectionModel>(Action<QueryStorePopulatorStartupOptions<TProjectionModel>> setup = null) 
            where TProjectionModel : class, IProjectionModel
        {
            return Projection(
                true, 
                setup);
        }
    }
}