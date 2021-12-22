using System.Collections;
using System.Collections.Generic;

namespace Next.Cqrs.Queries
{
    public interface IQueryPredicate
    {
        public IEnumerable<IQueryFilter> Filters { get; }
        
        IQueryPredicate AddFilter(
            string name,
            object value);
    }


    public interface IQueryFilter
    {
        string Name { get; }
        object Value { get; }
    }

    public class QueryFilter : IQueryFilter
    {
        public string Name { get; }
        public object Value { get; }

        public QueryFilter(
            string name,
            object value)
        {
            Name = name;
            Value = value;
        }
    }
}