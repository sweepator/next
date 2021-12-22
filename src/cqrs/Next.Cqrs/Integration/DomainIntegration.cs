using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Next.Abstractions.Domain;
using Next.Abstractions.Mapper;

namespace Next.Cqrs.Integration
{
    internal class DomainIntegration<TAggregate, TIdentity, TAggregateEvent, TIntegrationEvent> :
        IDomainIntegration<TAggregate, TIdentity, TAggregateEvent, TIntegrationEvent>, IDomainIntegration
        where TAggregate : IAggregateRoot<TIdentity>
        where TIdentity : IIdentity
        where TAggregateEvent : IAggregateEvent<TAggregate>
        where TIntegrationEvent : class
    {
        private readonly ILogger<DomainIntegration<TAggregate, TIdentity, TAggregateEvent, TIntegrationEvent>> _logger;
        private readonly IMapper _mapper;
        private readonly IOptions<DomainIntegrationOptions<TIntegrationEvent>> _options;
        private readonly IDomainIntegrationPublisher<TIntegrationEvent> _domainIntegrationPublisher;

        public DomainIntegration(
            ILogger<DomainIntegration<TAggregate, TIdentity, TAggregateEvent, TIntegrationEvent>> logger,
            IOptions<DomainIntegrationOptions<TIntegrationEvent>> options,
            IDomainIntegrationPublisher<TIntegrationEvent> domainIntegrationPublisher,
            IMapper mapper)
        {
            _logger = logger;
            _mapper = mapper;
            _options = options;
            _domainIntegrationPublisher = domainIntegrationPublisher;
        }

        public async Task Publish(
            TIntegrationEvent integrationEvent,
            IMetadata metadata)
        {
            try
            {
                await _domainIntegrationPublisher.Publish(
                    integrationEvent,
                    metadata);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error publishing integration event: {IntegrationEvent}", 
                    integrationEvent);
                throw;
            }
        }

        private TIntegrationEvent Map(IDomainEvent domainEvent)
        {
            try
            {
                return _options.Value?.MapFunc(domainEvent) ?? _mapper.Map<TIntegrationEvent>(domainEvent);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error mapping domain event to integration event: {DomainEvent} {IntegrationEventType}", 
                    domainEvent,
                    typeof(TIntegrationEvent).Name);
                throw;
            }
        }

        public async Task Publish(IDomainEvent domainEvent)
        {
            if (domainEvent.AggregateEvent.GetType() == typeof(TAggregateEvent))
            {
                var integrationEvent = Map((IDomainEvent<TAggregate, TIdentity, TAggregateEvent>)domainEvent);
                
                _logger.LogDebug("Publishing integration event: {IntegrationEvent}", integrationEvent);

                await Publish(
                    integrationEvent,
                    domainEvent.Metadata);

                _logger.LogDebug("Integration event published: {IntegrationEvent}", integrationEvent);
            }
        }
    }
}