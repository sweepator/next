using System;
using System.Text.Json;
using Next.Bus.Json;

namespace Next.Abstractions.Bus.Configuration
{
    public static class MessageBusConfiguratorExtensions
    {
        public static IMessageBusBuilder WithMessageJsonSerializer(
            this IMessageBusBuilder messageBusBuilder,
            Action<JsonSerializerOptions> setup = null)
        {
            var jsonSerializerOptions = Serialization.Json.JsonSerializerDefaults.GetDefaultSettings();
            
            setup?.Invoke(jsonSerializerOptions);

            var jsonSerializer = new Next.Abstractions.Serialization.Json.JsonSerializer(jsonSerializerOptions);
            messageBusBuilder.WithMessageSerializer(new MessageJsonSerializer(jsonSerializer));
            
            return messageBusBuilder;
        }
    }
}
