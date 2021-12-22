using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Next.Abstractions.Domain.Subscribers
{
    public interface IDomainEventSubscriber
    {
        Task Handle(
            IEnumerable<IDomainEvent> domainEvents,
            CancellationToken cancellationToken = default);
    }
}