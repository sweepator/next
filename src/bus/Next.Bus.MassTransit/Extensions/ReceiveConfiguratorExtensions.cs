using Next.Bus.MassTransit.Transport;

namespace MassTransit
{
    public static class ReceiveConfiguratorExtensions
    {
        public static IReceiveConfigurator<TEndpointConfigurator> ConfigureMessageBusTransport<TEndpointConfigurator>(
            this IReceiveConfigurator<TEndpointConfigurator> configurator,
            IBusRegistrationContext context)
            where TEndpointConfigurator : IReceiveEndpointConfigurator
        {
            var sendEndpointStrategy = context.GetRequiredService<ISendEndpointStrategy>();
            var endpoints = sendEndpointStrategy.GetConsumerEndpoints();
                        
            foreach (var endpoint in endpoints)
            {
                configurator.ReceiveEndpoint(
                    endpoint,
                    e =>
                    {
                        e.ConfigureConsumer<MassTransitTransport>(context);
                    });
            }

            return configurator;
        }
    }
}