using System;
using System.Collections.Generic;

namespace Next.Abstractions.Jobs
{
    public sealed class JobContext : IJobContext
    {
        public string JobId { get; }
        public Dictionary<string, object> Metadata { get; }

        public string RequestId { get; }

        public JobContext(string jobId)
        {
            RequestId = Guid.NewGuid().ToString();
            JobId = jobId;
            Metadata = new Dictionary<string, object>();
        }
    }
}
