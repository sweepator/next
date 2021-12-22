using Next.Abstractions.Jobs.Trace;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddJobTraceStrategy(this IServiceCollection services)
        {
            return services
                .AddTraceStrategy<JobTraceStrategy>();
        }
    }
}
