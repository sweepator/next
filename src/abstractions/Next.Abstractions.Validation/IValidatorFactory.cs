using System.Collections.Generic;

namespace Next.Abstractions.Validation
{
    public interface IValidatorFactory
    {
        IEnumerable<IValidator> GetValidators(object instance);
    }
}