using System;
using System.Collections.Generic;

namespace Next.Cqrs.Queries
{
    public class QueryPredicate : IQueryPredicate
    {
        private readonly List<IQueryFilter> _filters = new List<IQueryFilter>();

        public IEnumerable<IQueryFilter> Filters => _filters;
        
        public IQueryPredicate AddFilter(
            string name,
            object value)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }
            
            _filters.Add(new QueryFilter(name, value));
            return this;
        }
    }
}