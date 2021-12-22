using System.Collections.Generic;
using Next.Core.Specifications;

namespace Next.Abstractions.Domain.Specifications
{
    public class AggregateIsNewSpecification : Specification<IAggregateRoot>
    {
        protected override IEnumerable<string> IsNotSatisfiedBecause(IAggregateRoot obj)
        {
            if (!obj.IsNew)
            {
                yield return $"'{obj.Name}' with id '{obj.Id}' is not new.";
            }
        }
    }
}