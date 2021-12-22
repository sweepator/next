using System.Collections.Generic;
using System.Threading.Tasks;
using Next.Abstractions.Domain;

namespace Next.Abstractions.EventSourcing
{
    public interface IEventStoreBus
    {
        Task Publish(IDomainEvent @event);
        Task Publish(IEnumerable<IDomainEvent> events);
    }
}