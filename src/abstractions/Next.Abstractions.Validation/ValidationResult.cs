using System;
using System.Collections.Generic;
using System.Linq;
using Next.Core.Errors;

namespace Next.Abstractions.Validation
{
    public class ValidationResult
    {
        public static readonly ValidationResult Success = new();

        public bool IsValid => ValidationErrors == null || !ValidationErrors.Any();

        public IEnumerable<Error> ValidationErrors { get; }
        
        private ValidationResult()
        {
        }
        
        public ValidationResult(IEnumerable<Error> validationErrors)
        {
            ValidationErrors = validationErrors ?? throw new ArgumentNullException(nameof(validationErrors));
        }

        public static ValidationResult Error(
            string errorCode,
            string category = null,
            string message = null,
            IDictionary<string, object>  metadata = null)
        {
            return new(new[]
            {
                new Error(
                    errorCode,
                    category,
                    message,
                    metadata)
            });
        }
    }
}