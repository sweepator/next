using System;
using System.Linq.Expressions;
using Next.Cqrs.Queries.Projections;

namespace Next.Cqrs.Queries
{
    public abstract class QueryableRequest<TQueryResult> : QueryRequest<TQueryResult>
        where TQueryResult: IProjectionModel
    {
        public abstract Expression<Func<TQueryResult, bool>> GetQueryFilter();
    }
}