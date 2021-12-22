using Next.Abstractions.Data;
using Next.Cqrs.Queries.Projections;

namespace Next.Cqrs.Queries
{
    public abstract class QueryRequest<TProjectionModel> : IQueryRequest<TProjectionModel>
        where TProjectionModel : IProjectionModel
    {
        public virtual IQueryPredicate Map()
        {
            return new QueryPredicate();
        }
    }
}