using System;

namespace Next.Abstractions.Domain.Exceptions
{
    public class DomainException : Exception
    {
        public string ErrorCode { get; }

        public DomainException(string errorCode)
            : base($"{nameof(DomainException)}: {errorCode}")
        {
            ErrorCode = errorCode;
        }
    }
}