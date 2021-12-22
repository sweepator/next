using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Next.Abstractions.Data
{
    public class PagedList<T> : IPagedList<T>
    {
        private IEnumerable<T> Items { get; }
        
        public int PageNumber { get; }

        public int PageSize { get; }
        
        public long TotalCount { get; }
        
        public int TotalPages { get; }

        public bool HasPreviousPage => this.PageNumber > 1;

        public bool HasNextPage => PageNumber + 1 < TotalPages;
        
        public PagedList(
            IEnumerable<T> source, 
            int pageNumber, 
            int pageSize, 
            long totalCount)
        {
            PageNumber = pageNumber;
            PageSize = pageSize;

            var itemList = source.ToList();
            TotalCount = totalCount;
            TotalPages = (int)Math.Ceiling(TotalCount / (double)PageSize);
            Items = itemList.Skip((PageNumber - 1) * PageSize).Take(PageSize).ToList();
        }
        
        public PagedList(
            IQueryable<T> source, 
            int pageNumber, 
            int pageSize)
        {
            PageNumber = pageNumber;
            PageSize = pageSize;
            TotalCount = source.Count();
            TotalPages = (int)Math.Ceiling(TotalCount / (double)PageSize);
            Items = source.Skip((PageNumber - 1) * PageSize).Take(PageSize).ToList();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PagedList{T}" /> class
        /// </summary>
        public PagedList()
        {
            Items = Array.Empty<T>();
        }

        public IPagedList<TResult> ConvertTo<TResult>(Func<IEnumerable<T>, IEnumerable<TResult>> converter)
        {
            var pageList = new PagedList<TResult>
            (
                converter(Items),
                PageNumber,
                PageSize,
                TotalCount);
            return pageList;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return Items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}