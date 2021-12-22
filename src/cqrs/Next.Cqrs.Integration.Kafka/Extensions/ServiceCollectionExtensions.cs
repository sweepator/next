using System;
using Next.Abstractions.Domain;
using Next.Cqrs.Integration;
using Next.Cqrs.Integration.Kafka;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddKafkaDomainIntegration<TAggregateEvent, TIntegrationEvent>(
            this IServiceCollection services,
            Action<KafkaDomainIntegrationOptions<TIntegrationEvent>> kafkaSetup,
            Action<DomainIntegrationOptions<TIntegrationEvent>> setup = null)
            where TAggregateEvent : IAggregateEvent
            where TIntegrationEvent : class
        {
            if (kafkaSetup == null)
            {
                throw new ArgumentNullException(nameof(kafkaSetup));
            }
            
            services.AddDomainIntegration<TAggregateEvent, TIntegrationEvent, KafkaDomainPublisher<TIntegrationEvent>>(
                setup);
            
            services
                .AddOptions<KafkaDomainIntegrationOptions<TIntegrationEvent>>()
                .Configure(kafkaSetup);
            
            return services;
        }
    }
}