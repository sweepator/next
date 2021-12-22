using System.Reflection;
using Next.Cqrs.Queries.Projections;

namespace Next.Cqrs.Queries.MongoDb
{
    public interface IMongoDbProjectionModelDescriptionProvider
    {
        ProjectionModelDescription GetReadModelDescription<TProjectionModel>()
            where TProjectionModel : IProjectionModel;
    }
}