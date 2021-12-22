using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HotChocolate;
using Next.Cqrs.Queries;
using Next.Cqrs.Queries.Projections;

namespace Next.Web.Application.Graphql
{
    public class QueryResolvers<TQueryRequest, TProjection>
        where TQueryRequest:IQueryRequest<TProjection>
        where TProjection:IProjectionModel
    {
        public async Task<IEnumerable<TProjection>> Execute(
            TQueryRequest input,
            [Service] IQueryProcessor queryProcessor,
            CancellationToken cancellationToken)
        {
            return await queryProcessor.Execute(input, cancellationToken);
        }
    }
}