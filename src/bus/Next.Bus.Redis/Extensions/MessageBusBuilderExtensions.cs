using System;
using System.Text.Json;
using Next.Abstractions.Serialization.Json;
using Next.Bus.Redis.Subscriptions;
using Next.Bus.Redis.Transport;
using Next.Data.Redis;
using StackExchange.Redis;

namespace Next.Abstractions.Bus.Configuration
{
    public static class MessageBusBuilderExtensions
    {
        private const string NameSpace = "next";
        
        public static IMessageBusBuilder WithRedis(
            this IMessageBusBuilder messageBusBuilder,
            string connectionString,
            string prefix = null,
            Action<JsonSerializerOptions> jsonSetup = null)
        {
            return messageBusBuilder
                .WithRedisSubscriptionBroker(
                    connectionString, 
                    prefix)
                .WithRedisTransport(
                    connectionString, 
                    prefix,
                    jsonSetup)
                .WithRedisSubscriptionStore(
                    connectionString, 
                    prefix);
        }

        /// <summary>
        /// Configures the MessageBus to use a Redis transport
        /// </summary>
        /// <param name="messageBusBuilder">configurator</param>
        /// <param name="connectionString">redis connection</param>
        /// <param name="prefix">prefix for the redis keys</param>
        /// <param name="jsonSetup"></param>
        public static IMessageBusBuilder WithRedisTransport(
            this IMessageBusBuilder messageBusBuilder, 
            string connectionString, 
            string prefix = null,
            Action<JsonSerializerOptions> jsonSetup = null)
        {
            var jsonSerializerOptions = Serialization.Json.JsonSerializerDefaults.GetDefaultSettings();
            jsonSetup?.Invoke(jsonSerializerOptions);
            var jsonSerializer = new Serialization.Json.JsonSerializer(jsonSerializerOptions);
            
            return messageBusBuilder.WithCustomTransport(new RedisTransportFactory(
                new RedisConnectionFactory(),
                jsonSerializer,
                connectionString, 
                prefix));
        }
        
        /// <summary>
        /// Configures the MessageBus to use a Redis transport
        /// </summary>
        /// <param name="messageBusBuilder">configurator</param>
        /// <param name="connectionString">redis connection</param>
        /// <param name="prefix">prefix for the redis keys</param>
        /// <param name="jsonSerializer"></param>
        public static IMessageBusBuilder WithRedisTransport(
            this IMessageBusBuilder messageBusBuilder, 
            string connectionString, 
            IJsonSerializer jsonSerializer,
            string prefix = null)
        {
            return messageBusBuilder.WithCustomTransport(new RedisTransportFactory(
                new RedisConnectionFactory(),
                jsonSerializer,
                connectionString, 
                prefix));
        }

        /// <summary>
        /// Configures the MessageBus to use a Redis subscription store
        /// </summary>
        /// <param name="messageBusBuilder">configurator</param>
        /// <param name="connectionString">redis connection</param>
        /// <param name="prefix">prefix for the redis keys</param>
        public static IMessageBusBuilder WithRedisSubscriptionStore(
            this IMessageBusBuilder messageBusBuilder, 
            string connectionString, 
            string prefix = null)
        {
            prefix = string.IsNullOrWhiteSpace(prefix) ? NameSpace : $"{prefix}.{NameSpace}";

            return messageBusBuilder.WithCustomSubscriptionStore(new RedisSubscriptionStore(
                new RedisConnectionFactory(),
                connectionString, 
                prefix));
        }

        /// <summary>
        /// Configures the MessageBus to use a Redis subscription broker
        /// </summary>
        /// <param name="configurator">configurator</param>
        /// <param name="connectionString">redis connection</param>
        /// <param name="prefix">prefix for the redis keys</param>
        /// <returns>The current configurator instance to be used in a fluent manner</returns>
        public static IMessageBusBuilder WithRedisSubscriptionBroker(
            this IMessageBusBuilder configurator, 
            string connectionString, 
            string prefix = null) 
        {
            prefix = string.IsNullOrWhiteSpace(prefix) ? NameSpace : $"{prefix}.{NameSpace}";

            return configurator
                .WithCustomSubscriptionBroker(new RedisSubscriptionBroker(
                    new RedisConnectionFactory(),
                    connectionString,
                    prefix));
        }
    }
}
