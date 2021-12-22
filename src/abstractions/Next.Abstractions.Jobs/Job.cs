using System.Threading.Tasks;

namespace Next.Abstractions.Jobs
{
    public abstract class Job<TRequest> : IJob<TRequest>
        where TRequest : IJobRequest
    {
        public abstract Task Run(TRequest jobRequest);

        public async Task Run(IJobRequest request)
        {
            await Run((TRequest)request);
        }
    }
}

