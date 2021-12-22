using Hangfire;
using System;
using Next.Abstractions.Jobs;

namespace Next.Jobs.Hangfire
{
    internal sealed class JobService : IJobService
    {
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly IJobContextAccessor _jobContextAccessor;

        public JobService(
            IBackgroundJobClient backgroundJobClient,
            IJobContextAccessor jobContextAccessor)
        {
            _backgroundJobClient = backgroundJobClient;
            _jobContextAccessor = jobContextAccessor;
        }

        public string Schedule<TJob, TRequest>(
            TRequest request, 
            TimeSpan delay)
            where TJob : IJob<TRequest>
            where TRequest : IJobRequest
        {
            return _backgroundJobClient.Schedule<TJob>(
                o => o.Run(request), 
                delay);
        }
    }
}
