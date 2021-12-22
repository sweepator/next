using System.Collections.Generic;
using System.Threading.Tasks;
using Next.Abstractions.Domain;

namespace Next.Abstractions.EventSourcing
{
    public interface IEventStoreSyncProcessor
    {
        Task Process(IEnumerable<IDomainEvent> domainEvents);
    }
}