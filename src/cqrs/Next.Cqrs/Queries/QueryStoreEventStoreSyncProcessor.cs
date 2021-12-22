using System.Collections.Generic;
using System.Threading.Tasks;
using Next.Abstractions.Domain;
using Next.Abstractions.EventSourcing;

namespace Next.Cqrs.Queries
{
    public class QueryStoreEventStoreSyncProcessor<TSyncQueryStoreUpdater>: IEventStoreSyncProcessor
        where TSyncQueryStoreUpdater: ISyncQueryStoreUpdater
    {
        private readonly TSyncQueryStoreUpdater _syncQueryStoreUpdater;

        public QueryStoreEventStoreSyncProcessor(TSyncQueryStoreUpdater syncQueryStoreUpdater)
        {
            _syncQueryStoreUpdater = syncQueryStoreUpdater;
        }
        
        public async Task Process(IEnumerable<IDomainEvent> domainEvents)
        {
            await _syncQueryStoreUpdater.Update(domainEvents);
        }
    }
}