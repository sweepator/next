using System.Threading;
using System.Threading.Tasks;
using Next.Cqrs.Queries.Projections;

namespace Next.Cqrs.Queries
{
    public interface IQueryProcessor
    {
        Task<IQueryResponse<TProjectionModel>> Execute<TProjectionModel>(
            IQueryRequest<TProjectionModel> queryRequest,
            CancellationToken cancellationToken = default)
            where TProjectionModel : IProjectionModel;
    }
}