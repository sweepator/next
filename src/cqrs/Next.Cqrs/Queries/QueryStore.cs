using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Next.Abstractions.Domain;
using Next.Cqrs.Queries.Projections;

namespace Next.Cqrs.Queries
{
    public abstract class QueryStore<TProjectionModel> : IQueryStore<TProjectionModel>
        where TProjectionModel : class, IProjectionModel
    {
        public abstract Task<ProjectionModelEnvelope<TProjectionModel>> Get(
            object id,
            CancellationToken cancellationToken = default);
        
        public abstract Task Update(
            IEnumerable<ProjectionModelUpdate> updates,
            IProjectionModelContextFactory projectionModelContextFactory,
            Func<IProjectionModelContext, IEnumerable<IDomainEvent>, ProjectionModelEnvelope<TProjectionModel>, CancellationToken,
                Task<ProjectionModelUpdateResult<TProjectionModel>>> updateProjectionModel,
            CancellationToken cancellationToken = default);
        
        public abstract Task Delete(
            object id,
            CancellationToken cancellationToken = default);
        
        public abstract Task DeleteAll(CancellationToken cancellationToken = default);
    }
}