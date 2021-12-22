using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Next.Abstractions.Bus.Subscriptions;
using Next.Abstractions.Bus.Transport;

namespace Next.Abstractions.Bus.Configuration
{
    public interface IMessageBusBuilder
    {
        IServiceCollection Services { get; }
        
        ISubscriptionStore SubscriptionStore { get; }
        
        ISubscriptionBroker SubscriptionBroker { get; }
        
        ITransportFactory TransportFactory { get; }
        
        IMessageSerializer MessageSerializer { get; }
        
        IEnumerable<IProcessorBuilder> ProcessorConfigurators { get; }
        
        /// <summary>
        /// Creates a new ProcessorConfigurator to be configured as part of the current context
        /// </summary>
        /// <param name="setup">configuration handler</param>
        /// <returns>Current instance, to be used in a fluent manner</returns>
        IMessageBusBuilder WithProcessor(Action<IProcessorBuilder> setup);

        /// <summary>
        /// Configures a custom subscription broker
        /// </summary>
        /// <param name="subscriptionBroker">subscriptionBroker</param>
        /// <returns>Current instance, to be used in a fluent manner</returns>
        IMessageBusBuilder WithCustomSubscriptionBroker(ISubscriptionBroker subscriptionBroker);

        /// <summary>
        /// Configures a custom subscription store
        /// </summary>
        /// <param name="subscriptionStore">subscriptionStore</param>
        /// <returns>Current instance, to be used in a fluent manner</returns>
        IMessageBusBuilder WithCustomSubscriptionStore(ISubscriptionStore subscriptionStore);

        /// <summary>
        /// Configures a custom TransportFactory
        /// </summary>
        /// <param name="transportFactory">transportFactory</param>
        /// <returns>Current instance, to be used in a fluent manner</returns>
        IMessageBusBuilder WithCustomTransport(ITransportFactory transportFactory);

        /// <summary>
        /// Configures a custom TransportFactory
        /// </summary>
        /// <param name="transportFactoryFunc">transportFactory function receiving service provider</param>
        /// <returns>Current instance, to be used in a fluent manner</returns>
        IMessageBusBuilder WithCustomTransport(Func<IServiceProvider, ITransportFactory> transportFactoryFunc);

        /// <summary>
        /// Configures a custom message serializer
        /// </summary>
        /// <param name="messageSerializer">messageSerializer</param>
        /// <returns>Current instance, to be used in a fluent manner</returns>
        IMessageBusBuilder WithMessageSerializer(IMessageSerializer messageSerializer);
    }
}