using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using Next.Cqrs.Queries.Projections;

namespace Next.Cqrs.Queries.MongoDb
{
    public interface IMongoDbQueryStore<TProjectionModel> : IQueryableQueryStore<TProjectionModel>
        where TProjectionModel : class, IProjectionModel
    {
        Task<IAsyncCursor<TProjectionModel>> Find(
            Expression<Func<TProjectionModel, bool>> filter,
            FindOptions<TProjectionModel, TProjectionModel> options = null,
            CancellationToken cancellationToken = default);

        IQueryable<TProjectionModel> AsQueryable();
    }
}