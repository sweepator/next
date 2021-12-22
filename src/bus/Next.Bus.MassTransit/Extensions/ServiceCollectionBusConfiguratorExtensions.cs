using MassTransit.ExtensionsDependencyInjectionIntegration;
using Next.Bus.MassTransit.Transport;

namespace MassTransit
{
    public static class ServiceCollectionBusConfiguratorExtensions
    {
        public static IServiceCollectionBusConfigurator AddMessageBusTransport(
            this IServiceCollectionBusConfigurator serviceCollectionBusConfigurator)
        {
            serviceCollectionBusConfigurator.AddConsumer<MassTransitTransport>();
            return serviceCollectionBusConfigurator;
        }
    }
}