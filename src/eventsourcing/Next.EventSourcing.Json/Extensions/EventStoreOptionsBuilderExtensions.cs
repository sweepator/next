using System;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Next.Abstractions.Domain;
using Next.Abstractions.EventSourcing.Snapshot;
using Next.EventSourcing.Json;

namespace Next.Abstractions.EventSourcing
{
    public static class EventStoreOptionsBuilderExtensions
    {
        public static IEventStoreOptionsBuilder UseJsonSerializer(
            this IEventStoreOptionsBuilder eventStoreOptionsBuilder,
            Action<JsonSerializerOptions> setup = null)
        {
            var jsonSerializerOptions = Serialization.Json.JsonSerializerDefaults.GetDefaultSettings();
            
            setup?.Invoke(jsonSerializerOptions);

            var jsonSerializer = new Next.Abstractions.Serialization.Json.JsonSerializer(jsonSerializerOptions);
            
            eventStoreOptionsBuilder.Services.AddSingleton<IEventStoreSerializer>(sp => 
                new EventStoreJsonSerializer(
                    jsonSerializer,
                    sp.GetRequiredService<IAggregateEventDefinitionService>(),
                    sp.GetRequiredService<IDomainEventFactory>(),
                    sp.GetRequiredService<ISnapshotDefinitionService>()));
            
            return eventStoreOptionsBuilder;
        }
    }
}
