using Microsoft.Extensions.Hosting;

namespace Next.Abstractions.Health
{
    public interface IStartupTask : IHostedService
    {
        string Name { get; }
        
        bool IsComplete { get; }
    }
}
