using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Next.Abstractions.Bus.Subscriptions;
using Next.Abstractions.Bus.Transport;

namespace Next.Abstractions.Bus.Configuration
{
    public interface IProcessorBuilder
    {
        event Action<IServiceProvider, IProcessor> OnBuild;

        /// <summary>
        /// Sets the name of the endpoint for the current processor
        /// </summary>
        /// <param name="endpoint">endpoint name</param>
        /// <returns>The current ProcessorConfigurator instance, to be used in a fluent manner</returns>
        IProcessorBuilder Endpoint(string endpoint);
        
        /// <summary>
        /// Sets the value for max delivery count of each individual message until it gets deleted and deadlettered
        /// </summary>
        /// <param name="maxDeliveryCount">max delivery count</param>
        /// <returns>The current ProcessorConfigurator instance, to be used in a fluent manner</returns>
        IProcessorBuilder MaxDeliveryCount(int maxDeliveryCount);

        /// <summary>
        /// Sets whether messages should be deadlettered after reaching failing as many times as defined by MaxDeliveryCount
        /// </summary>
        /// <param name="deadLetterMessages">deadLetterMessages</param>
        /// <returns>The current ProcessorConfigurator instance, to be used in a fluent manner</returns>
        IProcessorBuilder DeadLetterMessages(bool deadLetterMessages);

        /// <summary>
        /// Sets the maximum number of concurrent messages being processed by this instance
        /// </summary>
        /// <remarks>
        /// On a scale out scenario, where multiple processes have the same Processor configured, the concurrencyLevel is per instance (process), not global per Processor definition
        /// </remarks>
        /// <param name="concurrencyLevel">concurrencyLevel</param>
        /// <returns>The current ProcessorConfigurator instance, to be used in a fluent manner</returns>
        IProcessorBuilder WithConcurrencyLevel(int concurrencyLevel);

        /// <summary>
        /// Registers a custom handler to handle messages of type TMessage.
        /// </summary>
        /// <typeparam name="TMessage">Type of the message to handle</typeparam>
        /// <param name="handlerName">handlerName</param>
        /// <param name="handlerFunc">handlerFunc</param>
        /// <returns>Current instance, to be used in a fluent manner</returns>
        IProcessorBuilder RegisterMessageHandler<TMessage>(
            Func<TMessage, Task> handlerFunc,
            string handlerName = null);

        /// <summary>
        /// Registers a custom handler to handle messages of type TMessage.
        /// </summary>
        /// <param name="messageType">messageType</param>
        /// <param name="handlerName">handlerName</param>
        /// <param name="messageHandler">messageHandler</param>
        /// <returns>Current instance, to be used in a fluent manner</returns>
        public IProcessorBuilder RegisterMessageHandler(
            Type messageType,
            IMessageHandler messageHandler,
            string handlerName = null);
        
        /// <summary>
        /// Build processor
        /// </summary>
        /// <param name="subscriptionManager"></param>
        /// <param name="transportFactory"></param>
        /// <param name="messageSerializer"></param>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        public IProcessor Build(
            ISubscriptionManager subscriptionManager,
            ITransportFactory transportFactory,
            IMessageSerializer messageSerializer,
            IServiceProvider serviceProvider);
    }
}