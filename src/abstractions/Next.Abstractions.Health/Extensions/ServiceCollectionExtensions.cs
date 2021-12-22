using System;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Next.Abstractions.Health;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        private static IServiceCollection AddStartupTaskContext(this IServiceCollection services)
        {
            services.TryAddSingleton(new StartupTaskContext());
            return services;
        }

        public static IServiceCollection AddStartupTask<T>(this IServiceCollection services)
            where T : class, IStartupTask
        {
            return services
                .AddStartupTaskContext()
                .AddHostedService<T>();
        }
        
        public static IServiceCollection AddStartupTask<T>(
            this IServiceCollection services,
            Func<IServiceProvider, T> implementationFactory)
            where T : class, IStartupTask
        {
            return services
                .AddStartupTaskContext()
                .AddHostedService(implementationFactory);
        }
    }
}
