using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Next.Abstractions.Domain;
using Next.Abstractions.EventSourcing.Metadata;
using Next.Abstractions.EventSourcing.Outbox;
using Next.Abstractions.EventSourcing.Snapshot;

namespace Next.Abstractions.EventSourcing
{
    public sealed class EventStoreOptionsBuilder : IEventStoreOptionsBuilder
    {
        /*private static readonly MethodInfo UseSingleAggregateRestoreMethod =
            typeof(EventStoreOptionsBuilder)
                .GetTypeInfo()
                .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                .Single(m => m.Name == nameof(UseProjectionStoreFor) && m.GetGenericArguments().Length == 4);*/
        
        private readonly List<Type> _aggregateEventTypes = new List<Type>();
        
        public IServiceCollection Services { get; }
        
        public EventStoreOptionsBuilder(IServiceCollection services)
        {
            Services = services;
        }

        public IEventStoreOptionsBuilder AddMetadataProvider<TMetadataProvider>() 
            where TMetadataProvider : class, IMetadataEnricher
        {
            Services.AddSingleton<IMetadataEnricher, TMetadataProvider>();
            return this;
        }

        public IEventStoreOptionsBuilder AddSnapshotStrategy<TSnapshotStrategy>()
            where TSnapshotStrategy : class, ISnapshotStrategy
        {
            Services.AddHostedService<SnapshotProcessorHostingService>();
            Services.TryAddSingleton<ISnapshotStrategy, TSnapshotStrategy>();
            return this;
        }

        public IEventStoreOptionsBuilder ConfigurePublisher(Action<EventPublisherOptions> setup)
        {
            if (setup == null)
            {
                throw new ArgumentNullException(nameof(setup));
            }
            
            Services.Configure(setup);
            Services.TryAddTransient<IOutboxStore, OutboxStore>();
            Services.TryAddSingleton<IOutboxStoreListener, OutboxStoreListener>();
            Services.AddHostedService<OutboxProcessorHostingService>();
            
            return this;
        }
        
        public IEventStoreOptionsBuilder Setup(Assembly assembly)
        {
            RegisterEvents(assembly);
            return this;
        }
        
        private void RegisterEvents(Assembly assembly)
        {
            var aggregateEventTypes = assembly
                .GetTypes()
                .Where(t => !t.GetTypeInfo().IsAbstract && typeof(IAggregateEvent).GetTypeInfo().IsAssignableFrom(t));
            
            _aggregateEventTypes.AddRange(aggregateEventTypes);
        }
        
        /*public IEventStoreOptionsBuilder UseProjectionStoreFor<TProjectionStore, TProjectionModel>()
            where TProjectionStore : class, IProjectionStore<TProjectionModel>
            where TProjectionModel : class, IProjectionModel
        {
            var (aggregateType, idType) = GetSingleAggregateTypes<TProjectionModel>();
            
            UseSingleAggregateRestoreMethod
                    .MakeGenericMethod(aggregateType, idType, typeof(TProjectionStore), typeof(TProjectionModel))
                    .Invoke(this, Array.Empty<object>());

            return this;
        }

        public IEventStoreOptionsBuilder UseProjectionStoreFor<TProjectionStore, TProjectionModel, TProjectionModelLocator>()
            where TProjectionStore : class, IProjectionStore<TProjectionModel>
            where TProjectionModel : class, IProjectionModel
            where TProjectionModelLocator : IProjectionModelLocator
        {
            Services
                .AddTransient<IProjectionStoreManager, MultipleProjectionStoreManager<TProjectionStore, TProjectionModel, TProjectionModelLocator>>();
                //.AddTransient<IQueryHandler<ReadModelByIdQuery<TProjectionModel>, TProjectionModel>, ReadModelByIdQueryHandler<TProjectionStore, TProjectionModel>>();
                
            return this;
        }
        
        public IEventStoreOptionsBuilder UseInMemoryProjectionStoreFor<TProjectionModel>()
            where TProjectionModel : class, IProjectionModel
        {
            RegisterInMemoryReadStore<TProjectionModel>();
            UseProjectionStoreFor<IInMemoryReadStore<TProjectionModel>, TProjectionModel>();
            return this;
        }
        
        public IEventStoreOptionsBuilder UseInMemoryProjectionStoreFor<TProjectionModel, TProjectionModelLocator>()
            where TProjectionModel : class, IProjectionModel
            where TProjectionModelLocator : IProjectionModelLocator
        {
            RegisterInMemoryReadStore<TProjectionModel>();
            UseProjectionStoreFor<IInMemoryReadStore<TProjectionModel>, TProjectionModel, TProjectionModelLocator>();
            return this;
        }
        
        
        
        private IEventStoreOptionsBuilder UseProjectionStoreFor<TAggregate, TIdentity, TProjectionModelStore, TProjectionModel>()
            where TAggregate : IAggregateRoot<TIdentity>
            where TIdentity : IIdentity
            where TProjectionModelStore : class, IProjectionStore<TProjectionModel>
            where TProjectionModel : class, IProjectionModel
        {
            Services
                .AddTransient<IProjectionStoreManager, SingleProjectionStoreManager<TAggregate, TIdentity, TProjectionModelStore, TProjectionModel>>();
                //.AddTransient<IQueryHandler<ReadModelByIdQuery<TReadModel>, TReadModel>, ReadModelByIdQueryHandler<TReadStore, TReadModel>>();
            
            return this;
        }
        
        private void RegisterInMemoryReadStore<TProjectionModel>()
            where TProjectionModel : class, IProjectionModel
        {
            Services.AddSingleton<IInMemoryReadStore<TProjectionModel>, InMemoryReadStore<TProjectionModel>>();
            Services.AddTransient<IProjectionStore<TProjectionModel>>(r => r.GetRequiredService<IInMemoryReadStore<TProjectionModel>>());
            //serviceCollection.AddTransient<IQueryHandler<InMemoryQuery<TReadModel>, IReadOnlyCollection<TReadModel>>, InMemoryQueryHandler<TReadModel>>();
        }
        
        private static (Type aggregateType, Type idType) GetSingleAggregateTypes<TProjectionModel>()
            where TProjectionModel : class, IProjectionModel
        {
            var readModelInterface = typeof(IProjectionModelFor<,,>);

            bool IsReadModelInterface(Type type)
            {
                var info = type.GetTypeInfo();
                if (!info.IsGenericType)
                {
                    return false;
                }
                
                var definition = info.GetGenericTypeDefinition();
                return definition == readModelInterface;
            }

            var readModelType = typeof(TProjectionModel);
            var results = readModelType
                .GetTypeInfo()
                .GetInterfaces()
                .Where(IsReadModelInterface)
                .GroupBy(i => new
                {
                    AggregateType = i.GenericTypeArguments[0],
                    IdType = i.GenericTypeArguments[1]
                })
                .ToList();

            if (!results.Any())
            {
                var message = $"You are trying to register ProjectionModel type {typeof(TProjectionModel).Name} which doesn't subscribe to any events. Implement the IProjectionModelFor<,,> interface.";
                throw new InvalidOperationException(message);
            }

            if (results.Count > 1)
            {
                var message = $"You are trying to register ProjectionModel type {typeof(TProjectionModel).Name} " +
                              "which subscribes to events from different aggregates. " +
                              "Use a ProjectionModelLocator, like this: " +
                              $"options.UseSomeReadStoreFor<{typeof(TProjectionModel)},MyReadModelLocator>";

                throw new InvalidOperationException(message);
            }

            var result = results.Single();
            return (result.Key.AggregateType, result.Key.IdType);
        }*/
    }
}
