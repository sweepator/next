using System.Collections;
using System.Collections.Generic;

namespace Next.Cqrs.Queries
{
    public class QueryResponse<TQueryResult> : IQueryResponse<TQueryResult>
    {
        private IEnumerable<TQueryResult> Result { get; }

        public QueryResponse(IEnumerable<TQueryResult> result)
        {
            Result = result;
        }

        public IEnumerator<TQueryResult> GetEnumerator()
        {
            return Result.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}