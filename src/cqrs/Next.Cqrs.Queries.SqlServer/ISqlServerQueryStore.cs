using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Next.Cqrs.Queries.Projections;

namespace Next.Cqrs.Queries.SqlServer
{
    public interface ISqlServerQueryStore<TProjectionModel> : IQueryStore<TProjectionModel>
        where TProjectionModel : class, IProjectionModel
    {
        Task<IEnumerable<TProjectionModel>> Find(
            DynamicParameters dynamicParameters,
            string where,
            CancellationToken cancellationToken= default);
    }
}