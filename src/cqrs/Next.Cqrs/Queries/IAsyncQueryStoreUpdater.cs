using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Next.Abstractions.Domain;
using Next.Cqrs.Queries.Projections;

namespace Next.Cqrs.Queries
{
    public interface IAsyncQueryStoreUpdater
    {
        Type ProjectionModelType { get; }

        Task Update(
            IEnumerable<IDomainEvent> domainEvents,
            CancellationToken cancellationToken = default);
    }

    public interface IAsyncQueryStoreUpdater<TProjectionModel> : IAsyncQueryStoreUpdater
        where TProjectionModel : class, IProjectionModel
    {
        new Type ProjectionModelType => typeof(TProjectionModel);

        Type IAsyncQueryStoreUpdater.ProjectionModelType => ProjectionModelType;
    }
}