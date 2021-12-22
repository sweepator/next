using System.Collections.Generic;

namespace Next.Abstractions.Jobs
{
    public interface IJobRequest
    {
        Dictionary<string, object> Metadata { get; }
        object Content { get; }
    }

    public interface IJobRequest<out TContent> : IJobRequest
    {
        new TContent Content { get; }
        
        object IJobRequest.Content => Content;
    }
}

