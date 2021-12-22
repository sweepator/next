using System.Threading;
using System.Threading.Tasks;
using Next.Application.Pipelines;
using Next.Cqrs.Queries.Projections;

namespace Next.Cqrs.Queries
{
    internal sealed class QueryProcessor : IQueryProcessor
    {
        private readonly IPipelineEngine _pipelineEngine;

        public QueryProcessor(IPipelineEngine pipelineEngine)
        {
            _pipelineEngine = pipelineEngine;
        }

        public async Task<IQueryResponse<TProjectionModel>> Execute<TProjectionModel>(
            IQueryRequest<TProjectionModel> queryRequest,
            CancellationToken cancellationToken = default)
            where TProjectionModel : IProjectionModel
        {
            return await _pipelineEngine.Execute(
                queryRequest,
                cancellationToken);
        }
    }
}