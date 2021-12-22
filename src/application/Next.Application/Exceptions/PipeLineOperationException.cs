using Next.Core.Errors;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Next.Application.Exceptions
{
    public class PipeLineOperationException : Exception
    {
        public PipeLineOperationException(IEnumerable<Error> errors)
            : base(string.Join(' ', errors.Select(e => e.Code)))
        {
            if (errors == null)
            {
                throw new ArgumentNullException(nameof(errors));
            }
        }
    }
}
