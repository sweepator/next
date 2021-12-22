using System.Collections.Concurrent;
using System.Collections.Generic;
using Next.Abstractions.Domain;

namespace Next.Abstractions.EventSourcing.Outbox
{
    public class OutboxStoreListener : IOutboxStoreListener
    {
        private static readonly ConcurrentQueue<IEnumerable<IDomainEvent>> Queue = new();
        
        public void NotifyEventToProcess(IEnumerable<IDomainEvent> domainEvents)
        {
            Queue.Enqueue(domainEvents);
        }

        public bool TryGetEventToProcess(out IEnumerable<IDomainEvent> domainEvents)
        {
            return Queue.TryDequeue(out domainEvents);
        }
    }
}