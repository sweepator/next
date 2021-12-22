using System;

namespace Next.Abstractions.Validation.Exceptions
{
    public class ValidationException : Exception
    {
        public ValidationException(Exception ex) 
            : this(null, ex)
        {

        }
        public ValidationException(
            string message, 
            Exception ex) 
            : base(message, ex)
        {

        }

        public ValidationException(string message) 
            : base(message)
        {

        }
    }
}