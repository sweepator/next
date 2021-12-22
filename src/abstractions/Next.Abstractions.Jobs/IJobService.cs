using System;

namespace Next.Abstractions.Jobs
{
    public interface IJobService
    {
        string Schedule<TJob, TRequest>(TRequest request, TimeSpan delay)
            where TJob : IJob<TRequest>
            where TRequest : IJobRequest;
    }
}

