using System.Collections.Generic;

namespace Next.Abstractions.Jobs
{
    public interface IJobContext
    {
        string RequestId { get; }
        string JobId { get; }
        Dictionary<string, object> Metadata { get; }
    }
}
