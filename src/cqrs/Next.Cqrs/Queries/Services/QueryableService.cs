using System.Collections.Generic;
using System.Threading.Tasks;
using Next.Cqrs.Queries.Projections;

namespace Next.Cqrs.Queries.Services
{
    public class QueryableService<TQueryRequest, TProjectionModel> : IQueryService<TQueryRequest, TProjectionModel>
        where TQueryRequest : QueryableRequest<TProjectionModel>
        where TProjectionModel: class, IProjectionModel
    {
        private readonly IQueryableQueryStore<TProjectionModel> _queryableQueryStore;

        public QueryableService(IQueryableQueryStore<TProjectionModel> queryableQueryStore)
        {
            _queryableQueryStore = queryableQueryStore;
        }
        
        public async Task<IEnumerable<TProjectionModel>> Get(TQueryRequest queryRequest)
        {
            var expression = queryRequest.GetQueryFilter();
            return await _queryableQueryStore.Find(expression);
        }
    }
}