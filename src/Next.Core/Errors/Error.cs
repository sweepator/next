using System.Collections.Generic;

namespace Next.Core.Errors
{
    public sealed class Error
    {
        public string Code { get; }
        public string Message { get; private set; }
        public string Category { get; private set; }
        public IDictionary<string, object> Metadata { get; }

        public Error(
            string code,
            string category = null,
            string message = null,
            IDictionary<string, object> metadata = null)
        {
            Code = code;
            Category = category;
            Message = message;
            Metadata = metadata;
        }

        public Error WithMessage(string message)
        {
            Message = message;
            return this;
        }
        
        public static implicit operator string(Error error) => error.Code;
        public static explicit operator Error(string errorCode) => new Error(errorCode);
    }
}
