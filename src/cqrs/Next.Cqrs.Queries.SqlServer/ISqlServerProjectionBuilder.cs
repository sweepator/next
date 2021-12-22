using System;
using Next.Cqrs.Queries.Projections;

namespace Next.Cqrs.Queries.SqlServer
{
    public interface ISqlServerProjectionBuilder
    {
        public ISqlServerProjectionBuilder AsyncProjection<TProjectionModel>(
            Action<QueryStorePopulatorStartupOptions<TProjectionModel>> setup = null)
            where TProjectionModel : class, IProjectionModel;
        
        public ISqlServerProjectionBuilder SyncProjection<TProjectionModel>(
            Action<QueryStorePopulatorStartupOptions<TProjectionModel>> setup = null)
            where TProjectionModel : class, IProjectionModel;
    }
}