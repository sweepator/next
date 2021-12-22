using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Next.Abstractions.Domain;
using Next.Cqrs.Queries.Projections;

namespace Next.Cqrs.Queries
{
    public interface ISyncQueryStoreUpdater<TProjectionModel>: ISyncQueryStoreUpdater
        where TProjectionModel : class, IProjectionModel
    {
        new Type ProjectionModelType => typeof(TProjectionModel);

        Type ISyncQueryStoreUpdater.ProjectionModelType => ProjectionModelType;
    }

    public interface ISyncQueryStoreUpdater
    {
        Type ProjectionModelType { get; }

        Task Update(
            IEnumerable<IDomainEvent> domainEvents,
            CancellationToken cancellationToken = default);
    }
}