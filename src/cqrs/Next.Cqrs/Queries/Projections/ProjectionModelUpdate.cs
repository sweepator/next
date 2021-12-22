using System.Collections.Generic;
using Next.Abstractions.Domain;

namespace Next.Cqrs.Queries.Projections
{
    public class ProjectionModelUpdate
    {
        public object Id { get; }
        public IReadOnlyCollection<IDomainEvent> DomainEvents { get; }

        public ProjectionModelUpdate(
            object id,
            IReadOnlyCollection<IDomainEvent> domainEvents)
        {
            Id = id;
            DomainEvents = domainEvents;
        }
    }
}