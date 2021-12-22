using Next.Abstractions.Health;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class HealthChecksBuilderExtensions
    {
        public static IHealthChecksBuilder AddStartupHealthCheck(this IHealthChecksBuilder healthChecksBuilder)
        {
            return
                healthChecksBuilder
                    .AddCheck<StartupTasksHealthCheck>(
                        "startup",
                        tags: new[] {"ready"});
        }
    }
}
