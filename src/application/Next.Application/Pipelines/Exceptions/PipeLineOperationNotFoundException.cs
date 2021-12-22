using System;

namespace Next.Application.Pipelines.Exceptions
{
    public class PipeLineOperationNotFoundException : Exception
    {
        public PipeLineOperationNotFoundException(string message)
            : base(message)
        {
        }
    }
}
