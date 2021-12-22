using Next.Jobs.Hangfire;
using Hangfire;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class HangfireServiceCollectionExtensions
    {
        public static IServiceCollection AddHangfireServerDefaults(
            this IServiceCollection app,
            string applicationName)
        {
            GlobalJobFilters.Filters.Add(new JobCoreServerAttribute());

            return app.AddHangfireServer((serviceProvider, o) =>
            {
                o.Activator = new ServiceProviderJobActivator(serviceProvider.GetService<IServiceScopeFactory>());
                o.ServerName = applicationName;
            });
        }
    }
}
