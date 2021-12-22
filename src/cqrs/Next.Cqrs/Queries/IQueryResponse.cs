using System.Collections;
using System.Collections.Generic;
using Next.Application.Contracts;

namespace Next.Cqrs.Queries
{
    public interface IQueryResponse: IResponse, IEnumerable
    {
    }
    
    public interface IQueryResponse<out TQueryResult>: IQueryResponse, IEnumerable<TQueryResult>
    {
    }
}