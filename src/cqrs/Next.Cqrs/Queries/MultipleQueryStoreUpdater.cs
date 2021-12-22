using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Next.Abstractions.Domain;
using Next.Cqrs.Queries.Projections;

namespace Next.Cqrs.Queries
{
    public class MultipleQueryStoreUpdater<TProjectionModelStore, TProjectionModel, TProjectionModelLocator> : QueryStoreUpdater<TProjectionModelStore, TProjectionModel>
        where TProjectionModelStore : IQueryStore<TProjectionModel>
        where TProjectionModel : class, IProjectionModel
        where TProjectionModelLocator: IProjectionModelLocator
    {
        private readonly TProjectionModelLocator _projectionModelLocator;
        private readonly IProjectionModelFactory<TProjectionModel> _projectionModelFactory;
        private readonly IProjectionModelDomainEventApplier _projectionModelDomainEventApplier;

        public MultipleQueryStoreUpdater(
            ILogger<MultipleQueryStoreUpdater<TProjectionModelStore, TProjectionModel, TProjectionModelLocator>> logger,
            TProjectionModelStore queryStore,
            TProjectionModelLocator projectionModelLocator,
            IProjectionModelFactory<TProjectionModel> projectionModelFactory,
            IProjectionModelDomainEventApplier projectionModelDomainEventApplier) 
            : base(logger, queryStore)
        {
            _projectionModelLocator = projectionModelLocator;
            _projectionModelFactory = projectionModelFactory;
            _projectionModelDomainEventApplier = projectionModelDomainEventApplier;
        }

        protected override IEnumerable<ProjectionModelUpdate> BuildProjectionModelUpdates(IEnumerable<IDomainEvent> domainEvents)
        {
            var updates = (
                from de in domainEvents
                let projectionModelIds = _projectionModelLocator.GetProjectionModelIds(de)
                from rid in projectionModelIds
                group de by rid into g
                select new ProjectionModelUpdate(
                    g.Key, 
                    g
                        .OrderBy(d => d.Timestamp)
                        .ThenBy(d => d.AggregateEvent)
                        .ToList())
            ).ToList();

            return updates;
        }

        protected override async Task<ProjectionModelUpdateResult<TProjectionModel>> Update(
            IProjectionModelContext projectionModelContext, 
            IEnumerable<IDomainEvent> domainEvents,
            ProjectionModelEnvelope<TProjectionModel> projectionModelEnvelope, 
            CancellationToken cancellationToken = default)
        {
            var projectionModel = projectionModelEnvelope.ProjectionModel;
            if (projectionModel == null)
            {
                projectionModel = await _projectionModelFactory.Create(
                        projectionModelEnvelope.Id,
                        cancellationToken)
                    .ConfigureAwait(false);
            }

            _projectionModelDomainEventApplier.UpdateProjectionModel(
                projectionModel,
                domainEvents,
                projectionModelContext);

            return projectionModelEnvelope.AsModifedResult(
                projectionModel,
                projectionModelEnvelope.Version.GetValueOrDefault() + 1
            );
        }
    }
}