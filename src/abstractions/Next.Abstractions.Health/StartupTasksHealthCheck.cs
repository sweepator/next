using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Next.Abstractions.Health
{
    public class StartupTasksHealthCheck : IHealthCheck
    {
        private readonly StartupTaskContext _context;

        public StartupTasksHealthCheck(StartupTaskContext context)
        {
            _context = context;
        }

        public Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return Task.FromResult(_context.IsComplete ? 
                HealthCheckResult.Healthy("Startup tasks completed") : 
                HealthCheckResult.Unhealthy("Startup tasks not completed"));
        }
    }
}
