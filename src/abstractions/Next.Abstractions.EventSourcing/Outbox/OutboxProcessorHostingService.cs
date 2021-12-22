using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Next.Abstractions.EventSourcing.Outbox
{
    public class OutboxProcessorHostingService : IHostedService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IEventStoreBus _eventStoreBus;
        private readonly IOutboxStoreListener _outboxStoreListener;
        private readonly IOptions<EventPublisherOptions> _options;
        private readonly ILogger<OutboxProcessorHostingService> _logger;
        private Timer _timer;

        public OutboxProcessorHostingService(
            IServiceScopeFactory serviceScopeFactory,
            IEventStoreBus eventStoreBus,
            IOutboxStoreListener outboxStoreListener,
            IOptions<EventPublisherOptions> options,
            ILogger<OutboxProcessorHostingService> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _eventStoreBus = eventStoreBus;
            _outboxStoreListener = outboxStoreListener;
            _options = options;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            if (_options.Value.BackgroundProcessorEnabled)
            {
                _timer = new Timer(
                    SendOutboxMessages, 
                    null, 
                    TimeSpan.Zero, 
                    TimeSpan.FromSeconds(_options.Value.BackgroundLockInSeconds));
            }
            
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(
                Timeout.Infinite, 
                0);
            
            return Task.CompletedTask;
        }
        
        private void SendOutboxMessages(object state)
        {
            _ = Process();
        }

        private async Task Process()
        {
            _logger.LogTrace("Checking events to publish...");
            
            if (!_outboxStoreListener.TryGetEventToProcess(out var notificationEvents))
            {
                _logger.LogTrace("No events to publish");
                return;
            }
            
            _logger.LogTrace("Notification received with events to publish: {DomainEventsCount}", notificationEvents.Count());
            
            using var scope = _serviceScopeFactory.CreateScope();
            var outboxStore = scope.ServiceProvider.GetRequiredService<IOutboxStore>();

            var domainEventsByTransaction = (await outboxStore.GetUnCommittedDomainEvents())
                .GroupBy(o => o.Metadata.TransactionId)
                .ToDictionary(o => o.Key, o => o.ToList());

            _logger.LogTrace(
                "Found {DomainEventsCount} events to publish",
                domainEventsByTransaction.Count);

            foreach (var transactionId in domainEventsByTransaction.Keys)
            {
                var domainEvents = domainEventsByTransaction[transactionId];
                var identity = domainEvents.Select(o => o.AggregateIdentity).First();
                var aggregateType = domainEvents.Select(o => o.AggregateType).First();

                _logger.LogDebug(
                    "Processing {DomainEventsCount} events for {AggregateType} with id {Identity} and transaction {TransactionId}",
                    domainEvents.Count,
                    aggregateType.Name,
                    identity.Value,
                    transactionId);
                
                try
                {
                    await _eventStoreBus.Publish(domainEvents);
                    await outboxStore.Commit(
                        identity,
                        transactionId);
                    
                    _logger.LogDebug(
                        "Processed {DomainEventsCount} events for {AggregateType} with id {Identity} and transaction {TransactionId}",
                        domainEvents.Count,
                        aggregateType.Name,
                        identity.Value,
                        transactionId);
                }
                catch (Exception e)
                {
                    _logger.LogError(e,
                        "Error on processing {DomainEventsCount} events for {AggregateType} with id {Identity} and transaction {TransactionId}",
                        domainEvents.Count,
                        aggregateType.Name,
                        identity.Value,
                        transactionId);
                    break;
                }
            }
        }
    }
}