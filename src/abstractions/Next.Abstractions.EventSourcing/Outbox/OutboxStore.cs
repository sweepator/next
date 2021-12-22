using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Next.Abstractions.Domain;

namespace Next.Abstractions.EventSourcing.Outbox
{
    public sealed class OutboxStore : IOutboxStore
    {
        private readonly IOutboxStoreRepository _outboxStoreRepository;
        private readonly IEventStoreSerializer _eventStoreSerializer;
        private readonly IOptions<EventPublisherOptions> _options;

        public OutboxStore(
            IOutboxStoreRepository outboxStoreRepository,
            IEventStoreSerializer eventStoreSerializer,
            IOptions<EventPublisherOptions> options)
        {
            _outboxStoreRepository = outboxStoreRepository;
            _eventStoreSerializer = eventStoreSerializer;
            _options = options;
        }
        
        public async Task<IEnumerable<IDomainEvent>> GetUnCommittedDomainEvents()
        {
            var serializedEvents = await _outboxStoreRepository.GetAllUnCommitted(_options.Value.BackgroundBatchSize);
            
            var domainEvents = serializedEvents
                .Select(e => _eventStoreSerializer.Deserialize(e))
                .ToList();
            
            return domainEvents;
        }

        public async Task Commit(
            IIdentity identity,
            Guid transactionId)
        {
            await _outboxStoreRepository.Commit(
                identity,
                transactionId);
        }
    }
}