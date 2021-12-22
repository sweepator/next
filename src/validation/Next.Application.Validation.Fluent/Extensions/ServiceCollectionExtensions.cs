using Next.Abstractions.Validation;
using Next.Application.Validation.Fluent;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAggregateValidators(
            this IServiceCollection services)
        {
            return services.AddSingleton<IValidator, AggregateCommandValidator>();
        }
    }
}