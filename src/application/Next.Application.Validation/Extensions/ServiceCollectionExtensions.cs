using System;
using Next.Application.Validation;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddValidationStep(
            this IServiceCollection services,
            Action<IValidationOptionsBuilder> setup = null)
        {
            services.AddScoped(typeof(MediatR.IPipelineBehavior<,>), typeof(ValidateStep<,>));
            setup?.Invoke(new ValidationOptionsBuilder(services));
            return services;
        }
    }
}
