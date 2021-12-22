using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Next.Cqrs.Queries.Projections;

namespace Next.Cqrs.Queries
{
    public interface IQueryableQueryStore<TProjectionModel>: IQueryStore<TProjectionModel>
        where TProjectionModel : class, IProjectionModel
    {
        Task<IEnumerable<TProjectionModel>> Find(
            Expression<Func<TProjectionModel, bool>> filter,
            CancellationToken cancellationToken= default);
        
        Task Delete(
            Expression<Func<TProjectionModel, bool>> filter,
            CancellationToken cancellationToken= default);
    }
}