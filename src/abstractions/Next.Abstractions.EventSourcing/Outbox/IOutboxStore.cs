using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Next.Abstractions.Domain;

namespace Next.Abstractions.EventSourcing.Outbox
{
    public interface IOutboxStore
    {
        Task<IEnumerable<IDomainEvent>> GetUnCommittedDomainEvents();

        Task Commit(
            IIdentity identity,
            Guid transactionId);
    }
}