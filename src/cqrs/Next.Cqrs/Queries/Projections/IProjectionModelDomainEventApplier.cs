using System.Collections.Generic;
using Next.Abstractions.Domain;

namespace Next.Cqrs.Queries.Projections
{
    public interface IProjectionModelDomainEventApplier
    {
        bool UpdateProjectionModel<TProjectionModel>(
            TProjectionModel projectionModel,
            IEnumerable<IDomainEvent> domainEvents,
            IProjectionModelContext projectionModelContext)
            where TProjectionModel : IProjectionModel;
    }
}