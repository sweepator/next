using Next.Abstractions.Trace;

namespace Next.Abstractions.Jobs.Trace
{
    internal class JobTraceStrategy : ITraceStrategy
    {
        private readonly IJobContextAccessor _jobContextAccessor;

        public JobTraceStrategy(IJobContextAccessor jobContextAccessor)
        {
            _jobContextAccessor = jobContextAccessor;
        }

        public TraceInfo GetTraceInfo()
        {
            var jobContext = _jobContextAccessor.Context;

            if (jobContext == null)
            {
                return null;
            }

            jobContext.Metadata.TryGetValue(nameof(TraceInfo.CorrelationId), out object correlationId);

            return new TraceInfo(
                jobContext.RequestId,
                correlationId?.ToString());
        }
    }
}
