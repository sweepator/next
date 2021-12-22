using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using Next.Abstractions.Domain;
using Next.Abstractions.Domain.Attributes;

namespace System
{
    public static class TypeExtensions
    {
        private static readonly ConcurrentDictionary<Type, string> AggregateNames = new ConcurrentDictionary<Type, string>();
        
        public static string GetAggregateName(this Type aggregateType)
        {
            return AggregateNames.GetOrAdd(
                aggregateType,
                t =>
                {
                    if (!typeof(IAggregateRoot).GetTypeInfo().IsAssignableFrom(aggregateType))
                    {
                        throw new ArgumentException($"Type '{aggregateType.Name}' is not an aggregate root");
                    }

                    return 
                        t.GetTypeInfo().GetCustomAttributes<AggregateNameAttribute>().SingleOrDefault()?.Name ??
                        t.Name;
                });
        }
    }
}