using System.Collections.Generic;

namespace Next.Core.Specifications
{
    public interface ISpecification<in T>
    {
        bool IsSatisfiedBy(T obj);

        IEnumerable<string> WhyIsNotSatisfiedBy(T obj);
    }
}