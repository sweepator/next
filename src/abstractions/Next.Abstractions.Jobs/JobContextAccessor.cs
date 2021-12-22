using System.Threading;

namespace Next.Abstractions.Jobs
{
    public sealed class JobContextAccessor : IJobContextAccessor
    {
        private static AsyncLocal<JobContextHolder> JobContextCurrent = new AsyncLocal<JobContextHolder>();

        public IJobContext Context
        {
            get
            {
                return JobContextCurrent.Value?.Context;
            }
            set
            {
                var holder = JobContextCurrent.Value;
                if (holder != null)
                {
                    // Clear current OperationContext trapped in the AsyncLocals, as its done.
                    holder.Context = null;
                }

                if (value != null)
                {
                    JobContextCurrent.Value = new JobContextHolder { Context = value };
                }
            }
        }

        private class JobContextHolder
        {
            public IJobContext Context;
        }
    }
}
