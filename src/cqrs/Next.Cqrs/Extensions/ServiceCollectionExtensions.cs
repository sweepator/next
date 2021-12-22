using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Next.Abstractions.Domain;
using Next.Abstractions.EventSourcing;
using Next.Abstractions.Mapper;
using Next.Application.Jobs;
using Next.Cqrs.Bus;
using Next.Cqrs.Commands;
using Next.Cqrs.Configuration;
using Next.Cqrs.Integration;
using Next.Cqrs.Queries;
using Next.Cqrs.Queries.Projections;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        private static readonly DomainEventFactory DomainEventFactory = new();
        
        public static ICqrsBuilder AddCqrs(
            this IServiceCollection services,
            Assembly assembly,
            Action<ICqrsBuilder> setup)
        {
            var domainMetadataInfo = new DomainMetadataInfo(assembly);
            var cqrsBuilder = new CqrsBuilder(
                services,
                DomainEventFactory,
                domainMetadataInfo);
            setup?.Invoke(cqrsBuilder);
            cqrsBuilder.Build(services);

            // create pipeline assemblies
            var pipeLineAssemblies = new List<Assembly>(new[]
            {
                assembly,
                typeof(QueryStoreNotificationHandler<>).Assembly,
                typeof(JobSchedulerNotificationHandler<,,>).Assembly
            });
            
            services.AddLazyResolution();

            // application layer
            services.AddApplicationPipeline(pipeLineAssemblies.ToArray());
            services.AddScoped<ICommandProcessor, CommandProcessor>();
            services.AddScoped<IQueryProcessor, QueryProcessor>();
            services.AddSingleton<IDomainEventBus, DomainEventBus>();
            services.AddSingleton<IEventStoreBus, DomainEventBus>();
            services.AddSingleton<ICommandBus, CommandBus>();
            
            // domain layer
            services.AddSingleton<IAggregateEventDefinitionService>(new AggregateEventDefinitionService(domainMetadataInfo.AggregateEventTypes));
            services.AddSingleton<IDomainEventFactory>(DomainEventFactory);
            services.AddSingleton<IDomainMetadataInfo>(domainMetadataInfo);

            // queries/projections
            services.TryAddTransient<IQueryStorePopulator, QueryStorePopulator>();
            services.TryAddSingleton(typeof(IProjectionModelFactory<>), typeof(ProjectionModelFactory<>));
            services.TryAddSingleton<IProjectionModelDomainEventApplier, ProjectionModelDomainEventApplier>();
            services.TryAddTransient<IQueryStorePopulator, QueryStorePopulator>();
            services.AddSingleton<IProjectionModelDefinitionService>(new ProjectionModelDefinitionService(domainMetadataInfo.ProjectionModelTypes));

            return cqrsBuilder;
        }

        public static IServiceCollection AddDomainIntegration<TAggregateEvent, TIntegrationEvent, TPublisher>(
            this IServiceCollection services,
            Action<DomainIntegrationOptions<TIntegrationEvent>> setup = null,
            TPublisher publisher = null)
            where TAggregateEvent : IAggregateEvent
            where TIntegrationEvent : class
            where TPublisher: class, IDomainIntegrationPublisher<TIntegrationEvent>
        {
            var aggregateEventType = typeof(TAggregateEvent);
            var integrationEventType = typeof(TIntegrationEvent);
            var aggregateRootType = aggregateEventType.BaseType?.GetGenericArguments().FirstOrDefault();

            if (aggregateRootType == null)
            {
                throw new InvalidOperationException("Invalid aggregate event type.");
            }

            var identityType = aggregateRootType
                .BaseType?
                .GetGenericArguments()
                .FirstOrDefault(o => typeof(IIdentity).GetTypeInfo().IsAssignableFrom(o));

            if (identityType == null)
            {
                throw new InvalidOperationException("Invalid aggregate event type.");
            }

            var domainIntegrationType = typeof(DomainIntegration<,,,>).MakeGenericType(
                aggregateRootType,
                identityType,
                aggregateEventType,
                integrationEventType);
            var loggerType = typeof(ILogger<>).MakeGenericType(domainIntegrationType);
            var optionsType = typeof(IOptions<>).MakeGenericType(typeof(DomainIntegrationOptions<TIntegrationEvent>));
            var publisherType = typeof(IDomainIntegrationPublisher<>).MakeGenericType(typeof(TIntegrationEvent));

            if (setup != null)
            {
                services
                    .AddOptions<DomainIntegrationOptions<TIntegrationEvent>>()
                    .Configure(setup);
            }

            if (publisher != null)
            {
                services.AddSingleton<IDomainIntegrationPublisher<TIntegrationEvent>>(publisher);
            }
            else
            {
                services.AddTransient<IDomainIntegrationPublisher<TIntegrationEvent>, TPublisher>();
            }

            services.AddSingleton(sp => (IDomainIntegration)Activator.CreateInstance(
                domainIntegrationType,
                sp.GetRequiredService(loggerType),
                sp.GetRequiredService(optionsType),
                sp.GetRequiredService(publisherType),
                sp.GetRequiredService<IMapper>()));

            return services;
        }
    }
}
