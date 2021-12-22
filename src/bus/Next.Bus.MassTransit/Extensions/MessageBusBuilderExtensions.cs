using System;
using Microsoft.Extensions.DependencyInjection;
using Next.Abstractions.Bus.Transport;
using Next.Bus.MassTransit.Configuration;
using Next.Bus.Masstransit.Transport;
using Next.Bus.MassTransit.Transport;

namespace Next.Abstractions.Bus.Configuration
{
    public static class MessageBusBuilderExtensions
    {
        /// <summary>
        /// Configures the MessageBus to use a RabbitMq transport
        /// </summary>
        /// <param name="messageBusBuilder"></param>
        /// <param name="setupConfiguration"></param>
        public static IMessageBusBuilder WithMassTransitTransport(
            this IMessageBusBuilder messageBusBuilder,
            Action<IMassTransitBusConfigurationBuilder> setupConfiguration = null)
        {
            messageBusBuilder
                .Services
                .ReplaceOrAddSingleTon<ITransportFactory, MassTransitTransportFactory>();
            
            var configuration = new MassTransitBusConfiguration();
            var configurationBuilder = new MassTransitBusConfigurationBuilder(configuration);
            setupConfiguration?.Invoke(configurationBuilder);

            messageBusBuilder
                .Services
                .AddSingleton(configuration);
            
            messageBusBuilder
                .Services
                .AddSingleton<ISendEndpointStrategy, SendEndpointNamingStrategy>();

            return messageBusBuilder;
        }
    }
}
