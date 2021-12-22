using System.Collections.Generic;
using System.Threading.Tasks;
using Next.Cqrs.Queries.Projections;

namespace Next.Cqrs.Queries.Services
{
    public interface IQueryService<in TQueryRequest, TProjectionModel>
        where TQueryRequest : IQueryRequest<TProjectionModel>
        where TProjectionModel: IProjectionModel
    {
        Task<IEnumerable<TProjectionModel>> Get(TQueryRequest queryRequest);
    }
}