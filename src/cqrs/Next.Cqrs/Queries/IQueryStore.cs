using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Next.Abstractions.Domain;
using Next.Cqrs.Queries.Projections;

namespace Next.Cqrs.Queries
{
    public interface IQueryStore
    {
        Type ProjectionModelType { get; }
        
        Task Delete(
            object id,
            CancellationToken cancellationToken = default);

        Task DeleteAll(CancellationToken cancellationToken = default);
    }
    
    public interface IQueryStore<TProjectionModel> : IQueryStore
        where TProjectionModel : class, IProjectionModel
    {
        Type IQueryStore.ProjectionModelType => typeof(TProjectionModel);

        Task<ProjectionModelEnvelope<TProjectionModel>> Get(
            object id,
            CancellationToken cancellationToken= default);

        Task Update(
            IEnumerable<ProjectionModelUpdate> updates,
            IProjectionModelContextFactory projectionModelContextFactory,
            Func<IProjectionModelContext, IEnumerable<IDomainEvent>, ProjectionModelEnvelope<TProjectionModel>, CancellationToken,
                Task<ProjectionModelUpdateResult<TProjectionModel>>> updateProjectionModel,
            CancellationToken cancellationToken = default);
    }
}