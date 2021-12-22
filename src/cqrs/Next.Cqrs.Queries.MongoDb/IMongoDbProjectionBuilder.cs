using System;
using Next.Cqrs.Queries.Projections;

namespace Next.Cqrs.Queries.MongoDb
{
    public interface IMongoDbProjectionBuilder
    {
        public IMongoDbProjectionBuilder SyncProjection<TProjectionModel>(
            Action<QueryStorePopulatorStartupOptions<TProjectionModel>> setup = null)
            where TProjectionModel : class, IProjectionModel;
        
        public IMongoDbProjectionBuilder SyncProjection<TProjectionModel, TProjectionModelLocator>(
            Action<QueryStorePopulatorStartupOptions<TProjectionModel>> setup = null)
            where TProjectionModel : class, IProjectionModel
            where TProjectionModelLocator : class, IProjectionModelLocator;
        
        public IMongoDbProjectionBuilder AsyncProjection<TProjectionModel>(
            Action<QueryStorePopulatorStartupOptions<TProjectionModel>> setup = null)
            where TProjectionModel : class, IProjectionModel;

        public IMongoDbProjectionBuilder AsyncProjection<TProjectionModel, TProjectionModelLocator>(
            Action<QueryStorePopulatorStartupOptions<TProjectionModel>> setup = null)
            where TProjectionModel : class, IProjectionModel
            where TProjectionModelLocator : class, IProjectionModelLocator;
    }
}