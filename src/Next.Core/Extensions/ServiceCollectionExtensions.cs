using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection ReplaceOrAdd<TService, TImplementation>(
            this IServiceCollection services,
            ServiceLifetime lifetime)
            where TService : class
            where TImplementation : class, TService
        {
            var descriptorToRemove = services.FirstOrDefault(d => d.ServiceType == typeof(TService));

            if (descriptorToRemove != null)
            {
                services.Remove(descriptorToRemove);
            }
            
            var descriptorToAdd = new ServiceDescriptor(typeof(TService), typeof(TImplementation), lifetime);
            services.Add(descriptorToAdd);

            return services;
        }
        
        public static IServiceCollection ReplaceOrAddSingleTon<TService, TImplementation>(
            this IServiceCollection services)
            where TService : class
            where TImplementation : class, TService
        {
            return services.ReplaceOrAdd<TService, TImplementation>(ServiceLifetime.Singleton);
        }
        
        public static IServiceCollection ReplaceOrAddSingleton<TService>(
            this IServiceCollection services,
            TService instance)
            where TService : class
        {
            var descriptorToRemove = services.FirstOrDefault(d => d.ServiceType == typeof(TService));

            if (descriptorToRemove != null)
            {
                services.Remove(descriptorToRemove);
            }
            
            var descriptorToAdd = new ServiceDescriptor(
                typeof(TService), 
                instance);
            services.Add(descriptorToAdd);

            return services;
        }
        
        public static IServiceCollection ReplaceOrAddSingleton<TService>(
            this IServiceCollection services,
            Func<IServiceProvider, TService> implementationFactory)
            where TService : class
        {
            var descriptorToRemove = services.FirstOrDefault(d => d.ServiceType == typeof(TService));

            if (descriptorToRemove != null)
            {
                services.Remove(descriptorToRemove);
            }
            
            services.TryAddSingleton(implementationFactory);

            return services;
        }
        
        public static IServiceCollection AddLazyResolution(this IServiceCollection services)
        {
            services.TryAddTransient(
                typeof(Lazy<>),
                typeof(LazilyResolved<>));

            return services;
        }

        private class LazilyResolved<T> : Lazy<T>
        {
            public LazilyResolved(IServiceProvider serviceProvider)
                : base(serviceProvider.GetRequiredService<T>)
            {
            }
        }
    }
}