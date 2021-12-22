using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Next.Abstractions.Domain.Persistence;
using Next.Abstractions.EventSourcing;
using Next.Abstractions.EventSourcing.Snapshot;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddEventStore(
            this IServiceCollection services,
            IEnumerable<Type> snapshotStateTypes,
            Action<IEventStoreOptionsBuilder> setup = null)
        {
            var eventStoreOptionsBuilder = new EventStoreOptionsBuilder(services);
            setup?.Invoke(eventStoreOptionsBuilder);

            services.AddTransient<IEventStore, EventStore>();
            services.TryAddTransient(typeof(IAggregateRepository<,,>), typeof(AggregateStore<,,>));
            services.TryAddSingleton<ISnapshotProcessor, SnapshotProcessor>();
            services.AddSingleton<ISnapshotDefinitionService>(new SnapshotDefinitionService(snapshotStateTypes));
            
            return services;
        }
    }
}
