using System;
using System.Collections.Generic;
using System.Linq;
using Next.Core.Errors;

namespace Next.Cqrs.Exceptions
{
    public class CommandException : Exception
    {
        public CommandException(IEnumerable<Error> errors)
            : base(string.Join(' ', errors.Select(e => e.Code)))
        {
        }
    }
}