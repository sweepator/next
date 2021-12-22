using System;
using Next.Abstractions.Validation.Exceptions;

namespace Next.Abstractions.Validation
{
    public abstract class Validator<T> : IValidator<T>
    {
        public virtual bool CanValidate(Type type)
        {
            return typeof(T).IsAssignableFrom(type);
        }

        public abstract ValidationResult Validate(T input);

        public ValidationResult Validate(object input)
        {
            if (input == null)
            {
                throw new ValidationException("Input expected.");
            }

            if (input is T convertedInput)
            {
                return Validate(convertedInput);
            }

            throw new ValidationException($"Input type expected: {typeof(T)}. Input provided: {input.GetType()}");
        }
    }
}