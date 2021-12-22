using System.Collections.Generic;
using Next.Abstractions.Domain;

namespace Next.Abstractions.EventSourcing.Outbox
{
    public interface IOutboxStoreListener
    {
        void NotifyEventToProcess(IEnumerable<IDomainEvent> domainEvents);
        bool TryGetEventToProcess(out IEnumerable<IDomainEvent> domainEvents);
    }
}