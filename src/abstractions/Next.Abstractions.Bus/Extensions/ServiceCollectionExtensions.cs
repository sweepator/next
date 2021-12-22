using Microsoft.Extensions.DependencyInjection.Extensions;
using Next.Abstractions.Bus;
using Next.Abstractions.Bus.Configuration;
using Next.Abstractions.Bus.Subscriptions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMessageBus(
            this IServiceCollection services)
        {
            return services
                .AddHostedService<MessageHostingService>()
                .AddSingleton<IMessageBus, MessageBus>();
        }
    }
}