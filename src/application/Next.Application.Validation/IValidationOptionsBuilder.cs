using System;

namespace Next.Application.Validation
{
    public interface IValidationOptionsBuilder
    {
        IValidationOptionsBuilder Config<TRequest>(Action<ValidationStepOptions> setup);
    }
}
