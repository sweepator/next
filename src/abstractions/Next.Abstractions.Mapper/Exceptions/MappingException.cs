using System;

namespace Next.Abstractions.Mapper.Exceptions
{
    public class MappingException : Exception
    {
        public MappingException(Exception ex) 
            : this(null, ex)
        {

        }
        public MappingException(string message, Exception ex) 
            : base(message, ex)
        {

        }

        public MappingException(string message) 
            : base(message)
        {

        }
    }
}
