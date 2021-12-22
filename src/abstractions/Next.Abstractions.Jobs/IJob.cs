using System.Threading.Tasks;

namespace Next.Abstractions.Jobs
{
    public interface IJob
    {
        Task Run(IJobRequest request);
    }

    public interface IJob<in T> : IJob
        where T : IJobRequest
    {
        Task Run(T jobRequest);
    }
}

