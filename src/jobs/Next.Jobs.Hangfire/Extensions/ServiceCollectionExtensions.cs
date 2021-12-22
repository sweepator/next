using Next.Jobs.Hangfire;
using Next.Abstractions.Jobs;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddHangfireJobScheduler(this IServiceCollection services)
        {
            services.TryAddSingleton<IJobService, JobService>();
            services.TryAddSingleton<IJobContextAccessor, JobContextAccessor>();
            return services;
        }
    }
}
