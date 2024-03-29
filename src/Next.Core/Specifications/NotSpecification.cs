using System;
using System.Collections.Generic;

namespace Next.Core.Specifications
{
    public class NotSpecification<T> : Specification<T>
    {
        private readonly ISpecification<T> _specification;

        public NotSpecification(ISpecification<T> specification)
        {
            _specification = specification ?? throw new ArgumentNullException(nameof(specification));
        }

        protected override IEnumerable<string> IsNotSatisfiedBecause(T obj)
        {
            if (_specification.IsSatisfiedBy(obj))
            {
                yield return $"Specification '{_specification.GetType().Name}' should not be satisfied";
            }
        }
    }
}