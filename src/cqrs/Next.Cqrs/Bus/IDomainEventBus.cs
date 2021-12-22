using System.Collections.Generic;
using System.Threading.Tasks;
using Next.Abstractions.Domain;

namespace Next.Cqrs.Bus
{
    public interface IDomainEventBus
    {
        Task Publish(IEnumerable<IDomainEvent> events);
    }
}