using System.Threading;
using System.Threading.Tasks;
using Next.Application.Pipelines;
using Next.Cqrs.Queries.Projections;
using Next.Cqrs.Queries.Services;

namespace Next.Cqrs.Queries
{
    public sealed class QueryHandler<TQueryRequest, TProjectionModel> : RequestHandler<TQueryRequest, IQueryResponse<TProjectionModel>>
        where TQueryRequest : IQueryRequest<TProjectionModel>
        where TProjectionModel : IProjectionModel
    {
        private readonly IQueryService<TQueryRequest, TProjectionModel> _queryService;

        public QueryHandler(IQueryService<TQueryRequest, TProjectionModel> queryService)
        {
            _queryService = queryService;
        }
        
        public override async Task<IQueryResponse<TProjectionModel>> Execute(TQueryRequest request, 
            IOperationContext context, 
            CancellationToken cancellationToken = default)
        {
            var result = await _queryService.Get(request);
            return new QueryResponse<TProjectionModel>(result);
        }
    }
} 