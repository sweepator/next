using System.Collections.Generic;

namespace Next.Abstractions.Jobs
{
    public class JobRequest<T> : IJobRequest<T>
    {
        public Dictionary<string, object> Metadata { get; set; }
        
        public T Content { get; set; }
    }
}

