using System;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Next.Abstractions.Bus.Transport;
using Next.Bus.Kafka.Configuration;
using Next.Bus.Kafka.Transport;

namespace Next.Abstractions.Bus.Configuration
{
    public static class MessageBusBuilderExtensions
    {
        /// <summary>
        /// Configures the MessageBus to use a Kafka transport
        /// </summary>
        /// <param name="messageBusBuilder"></param>
        /// <param name="setup"></param>
        /// <param name="jsonSetup"></param>
        public static IMessageBusBuilder WithKafkaTransport(
            this IMessageBusBuilder messageBusBuilder,
            Action<IKafkaConfigurationBuilder> setup,
            Action<JsonSerializerOptions> jsonSetup = null)
        {
            if (setup == null)
            {
                throw new ArgumentNullException(nameof(setup));
            }
            
            var jsonSerializerOptions = Serialization.Json.JsonSerializerDefaults.GetDefaultSettings();
            jsonSetup?.Invoke(jsonSerializerOptions);
            var jsonSerializer = new Serialization.Json.JsonSerializer(jsonSerializerOptions);

            var kafkaConfiguration = new KafkaConfiguration();
            var kafkaConfigurationBuilder = new KafkaConfigurationBuilder(kafkaConfiguration);
            setup?.Invoke(kafkaConfigurationBuilder);

            messageBusBuilder
                .Services
                .AddSingleton<ITopicNamingStrategy, EndpointTopicNamingStrategy>();
            
            messageBusBuilder
                .Services
                .AddSingleton(sp => new Lazy<IEnumerable<IProcessor>>(sp.GetRequiredService<IEnumerable<IProcessor>>));
            
            return messageBusBuilder.WithCustomTransport(sp => new KafkaTransportFactory(
                sp.GetRequiredService<ILoggerFactory>(),
                kafkaConfiguration,
                sp.GetRequiredService<ITopicNamingStrategy>(),
                jsonSerializer));
        }
    }
}
