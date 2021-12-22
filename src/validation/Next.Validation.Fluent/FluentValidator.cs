using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using FluentValidation.Results;
using Next.Core.Errors;
using ValidationResult=Next.Abstractions.Validation.ValidationResult;

namespace Next.Validation.Fluent
{
    public abstract class FluentValidator<T> : AbstractValidator<T>, Next.Abstractions.Validation.IValidator<T>
    {
        public FluentValidator()
        {
            CascadeMode = CascadeMode.Continue;
        }

        public bool CanValidate(Type type)
        {
            return typeof(T).IsAssignableFrom(type);
        }

        public new ValidationResult Validate(T input)
        {
            var result = base.Validate(input);
            return new ValidationResult(result.Errors.Select(GetError));
        }

        public ValidationResult Validate(object input)
        {
            return Validate((T)input);
        }

        private static Error GetError(ValidationFailure validationFailure)
        {
           var metadata = new Dictionary<string, object>();

           if (validationFailure.CustomState != null)
           {
               metadata.Add("State", validationFailure.CustomState);
           }

           if (!string.IsNullOrEmpty(validationFailure.PropertyName))
           {
               metadata.Add("PropertyName", validationFailure.PropertyName);
           }

           return new Error(
                validationFailure.ErrorCode, 
                "Validation",
                validationFailure.ErrorMessage,
                metadata);
        }
    }
}