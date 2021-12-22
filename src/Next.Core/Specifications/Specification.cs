using System.Collections.Generic;
using System.Linq;

namespace Next.Core.Specifications
{
    public abstract class Specification<T> : ISpecification<T>
    {
        public bool IsSatisfiedBy(T obj)
        {
            return !IsNotSatisfiedBecause(obj).Any();
        }

        public IEnumerable<string> WhyIsNotSatisfiedBy(T obj)
        {
            return IsNotSatisfiedBecause(obj);
        }

        protected abstract IEnumerable<string> IsNotSatisfiedBecause(T obj);
    }
}