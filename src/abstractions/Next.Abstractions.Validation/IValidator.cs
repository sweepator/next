using System;

namespace Next.Abstractions.Validation
{
    public interface IValidator<in T>: IValidator
    {
        ValidationResult Validate(T input);
    }

    public interface IValidator
    {
        ValidationResult Validate(object input);
        bool CanValidate(Type type);
    }
}