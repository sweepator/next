using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Next.Web.Hypermedia;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddHypermedia(
            this IServiceCollection services,
            Assembly assembly,
            Action<LinksOptions> configure)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }
            
            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }
            
            services.Configure(configure);
            
            services.AddSingleton<IHateoasAssemblyLoader>(new HateoasAssemblyLoader(assembly));
            services.TryAddSingleton<IHateoasHandlerContextFactory, HateoasHandlerContextFactory>();
            services.TryAddSingleton<IHateoasRouteMap, HateoasRouteMap>();
            services.TryAddSingleton<ILinksPolicyProvider, LinksPolicyProvider>();
            services.TryAddSingleton<ILinksService, LinksService>();
            services.TryAddSingleton<ILinkTransformationContextFactory, LinkTransformationContextFactory>();
            services.TryAddSingleton<ILinksEvaluator, LinksEvaluator>();
            services.TryAddEnumerable(ServiceDescriptor.Singleton<ILinksHandler, PassThroughLinksHandler>());
            return services;
        }
    }
}