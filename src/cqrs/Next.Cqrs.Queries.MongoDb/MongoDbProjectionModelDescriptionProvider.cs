using System;
using System.Collections.Concurrent;
using Next.Cqrs.Queries.Projections;

namespace Next.Cqrs.Queries.MongoDb
{
    public class MongoDbProjectionModelDescriptionProvider : IMongoDbProjectionModelDescriptionProvider
    {
        private static readonly ConcurrentDictionary<Type, ProjectionModelDescription> CollectionNames = new();

        public ProjectionModelDescription GetReadModelDescription<TProjectionModel>() 
            where TProjectionModel : IProjectionModel
        {
            return CollectionNames.GetOrAdd(
                typeof(TProjectionModel),
                t =>
                {
                    var indexName = $"next.{typeof(TProjectionModel).Name.ToLower()}";
                    return new ProjectionModelDescription(new RootCollectionName(indexName));
                });

        }
    }
}