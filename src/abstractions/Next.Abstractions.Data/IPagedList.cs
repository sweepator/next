using System;
using System.Collections;
using System.Collections.Generic;

namespace Next.Abstractions.Data
{
    public interface IPagedList: IEnumerable
    {
        /// <summary>
        /// Selected page number
        /// </summary>
        int PageNumber { get; }
        
        int PageSize { get; }
        
        long TotalCount { get; }
        
        int TotalPages { get; }
        
        bool HasPreviousPage { get; }
        
        bool HasNextPage { get; }
    }

    /// <summary>
    /// Provides the interface(s) for paged list of any type
    /// </summary>
    /// <typeparam name="T">The type for paging.</typeparam>
    public interface IPagedList<out T> : IPagedList, IEnumerable<T>
    {
        IPagedList<TResult> ConvertTo<TResult>(Func<IEnumerable<T>, IEnumerable<TResult>> converter);
    }
}