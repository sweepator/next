using System;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Next.Application.Throttling;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddValidationStep(
            this IServiceCollection services,
            Action<IThrottlingOptionsBuilder> setup = null)
        {
            services.AddScoped(typeof(MediatR.IPipelineBehavior<,>), typeof(ThrottlingStep<,>));
            services.TryAddTransient<IIdentityRequestCache, InMemoryIdentityRequestCache>();
            setup?.Invoke(new ThrottlingOptionsBuilder(services));
            return services;
        }
    }
}
