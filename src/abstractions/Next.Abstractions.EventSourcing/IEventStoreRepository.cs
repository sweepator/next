using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Next.Abstractions.Data;
using Next.Abstractions.Domain;

namespace Next.Abstractions.EventSourcing
{
    public interface IEventStoreRepository
    {
        Task<IEnumerable<ISerializedEvent>> Get(IIdentity id);
        
        Task<IEnumerable<ISerializedEvent>> GetRange(
            IIdentity id,
            int start,
            int? end = null);

        Task<IEnumerable<ISerializedEvent>> GetUncommitted(IIdentity id);

        Task Save(
            IIdentity id,
            Guid transactionId,
            long expectedVersion,
            IEnumerable<ISerializedEvent> events,
            Func<Task> afterSaveCallback);

        Task Commit(
            IIdentity id,
            Guid transactionId);

        Task<IPagedList<ISerializedEvent>> GetCommitted(PageSelection pageSelection);
    }
}
