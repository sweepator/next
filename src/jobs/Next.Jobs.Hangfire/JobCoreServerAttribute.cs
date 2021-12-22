using Hangfire.Common;
using Hangfire.Server;
using System.Linq;
using Next.Abstractions.Jobs;

namespace Next.Jobs.Hangfire
{
    public class JobCoreServerAttribute : JobFilterAttribute, IServerFilter
    {
        private static readonly IJobContextAccessor JobContextAccessor = new JobContextAccessor();

        public void OnPerformed(PerformedContext filterContext)
        {
            JobContextAccessor.Context = null;
        }

        public void OnPerforming(PerformingContext filterContext)
        {
            var jobContext= new JobContext(filterContext.BackgroundJob.Id);
            JobContextAccessor.Context = jobContext;

            var request = (IJobRequest)filterContext.BackgroundJob.Job.Args.First();

            if (request.Metadata != null)
            {
                foreach (var key in request.Metadata.Keys)
                {
                    jobContext.Metadata.Add(key, request.Metadata[key]);
                }
            }
        }
    }
}
