using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Next.Abstractions.Domain;

namespace Next.Abstractions.EventSourcing.Outbox
{
    public interface IOutboxStoreRepository
    {
        Task<IEnumerable<ISerializedEvent>> GetAllUnCommitted(int limit);

        Task Commit(
            IIdentity id,
            Guid transactionId);
    }
}