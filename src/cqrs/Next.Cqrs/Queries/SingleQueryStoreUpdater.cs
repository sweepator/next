using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Next.Abstractions.Domain;
using Next.Abstractions.EventSourcing;
using Next.Cqrs.Queries.Projections;

namespace Next.Cqrs.Queries
{
    public class SingleQueryStoreUpdater<TAggregate, TIdentity, TProjectionModelStore, TProjectionModel> : QueryStoreUpdater<TProjectionModelStore, TProjectionModel>
        where TProjectionModelStore : IQueryStore<TProjectionModel>
        where TProjectionModel : class, IProjectionModel
        where TAggregate : IAggregateRoot<TIdentity>
        where TIdentity : IIdentity
    {
        private readonly ILogger<SingleQueryStoreUpdater<TAggregate, TIdentity, TProjectionModelStore, TProjectionModel>> _logger;
        private readonly IProjectionModelFactory<TProjectionModel> _projectionModelFactory;
        private readonly IProjectionModelDomainEventApplier _projectionModelDomainEventApplier;
        private readonly IEventStore _eventStore;

        public SingleQueryStoreUpdater(
            ILogger<SingleQueryStoreUpdater<TAggregate, TIdentity, TProjectionModelStore, TProjectionModel>> logger,
            TProjectionModelStore queryStore,
            IProjectionModelFactory<TProjectionModel> projectionModelFactory,
            IProjectionModelDomainEventApplier projectionModelDomainEventApplier,
            IEventStore eventStore) 
            : base(logger, queryStore)
        {
            _logger = logger;
            _projectionModelFactory = projectionModelFactory;
            _projectionModelDomainEventApplier = projectionModelDomainEventApplier;
            _eventStore = eventStore;
        }

        protected override IEnumerable<ProjectionModelUpdate> BuildProjectionModelUpdates(IEnumerable<IDomainEvent> domainEvents)
        {
            return domainEvents
                .GroupBy(d => d.AggregateIdentity.Value)
                .Select(g => new ProjectionModelUpdate(g.Key, g.OrderBy(d => d.Version).ToList()))
                .ToList();
        }

        protected override async Task<ProjectionModelUpdateResult<TProjectionModel>> Update(
            IProjectionModelContext projectionModelContext,
            IEnumerable<IDomainEvent> domainEvents,
            ProjectionModelEnvelope<TProjectionModel> projectionModelEnvelope,
            CancellationToken cancellationToken = default)
        {
            if (!domainEvents.Any())
            {
                throw new ArgumentException("No domain events");
            }

            IEnumerable<IDomainEvent> eventsToApply = null;
            var envelopeVersion = projectionModelEnvelope.Version;
            var expectedVersion = domainEvents.Min(d => d.Version);

            if (!envelopeVersion.HasValue)
            {
                eventsToApply = domainEvents;
                _logger.LogDebug(
                    "Projection model {ProjectionModelType} with id {Id} has version {Version} (or none), applying events",
                    typeof(TProjectionModel).Name,
                    projectionModelEnvelope.Id,
                    expectedVersion);
            }
            else if (expectedVersion != envelopeVersion)
            {
                var version = envelopeVersion.Value;

                if (domainEvents.Any(d => d.Version > envelopeVersion))
                {
                    expectedVersion = domainEvents.Where(d => d.Version > version)
                        .Select(d => d.Version)
                        .Min();
                }

                if (expectedVersion <= version)
                {
                    _logger.LogDebug(
                        "Projection model {ProjectionModelType} with id {Id} already has version {Version} compared to {ExpectedVersion}, skipping",
                        typeof(TProjectionModel).Name,
                        projectionModelEnvelope.Id,
                        version,
                        expectedVersion);
                    return projectionModelEnvelope.AsUnmodifedResult();
                }

                // Apply missing events
                var identity = domainEvents.Cast<IDomainEvent<TAggregate, TIdentity>>()
                    .First()
                    .AggregateIdentity;
                
                eventsToApply = await _eventStore.LoadRange<TAggregate, TIdentity>(
                        identity,
                        version + 1)
                    .ConfigureAwait(false);

                _logger.LogDebug(
                    "Projection model {ProjectionModelType} with id {Id} is missing some events {Version} < {ExpectedVersion}, adding them (got {EventsCount} events)",
                    typeof(TProjectionModel).Name,
                    projectionModelEnvelope.Id,
                    version,
                    expectedVersion,
                    eventsToApply.Count());
            }

            if (eventsToApply == null)
            {
                return projectionModelEnvelope.AsUnmodifedResult();
            }

            /*if (!domainEvents.Any())
            {
                throw new ArgumentException("No domain events");
            }

            var expectedVersion = domainEvents.Min(d => d.Version) - 1;
            var envelopeVersion = projectionModelEnvelope.Version;

            IEnumerable<IDomainEvent> eventsToApply = null;

            if (envelopeVersion.HasValue && expectedVersion != envelopeVersion)
            {
                var version = envelopeVersion.Value;
                if (expectedVersion < version)
                {
                    return projectionModelEnvelope.AsUnmodifedResult();
                }

                // Apply missing events
                var identity = domainEvents.Cast<IDomainEvent<TAggregate, TIdentity>>().First().AggregateIdentity;
                eventsToApply = await _eventStore.LoadRange<TAggregate, TIdentity>(
                        identity,
                        version + 1)
                    .ConfigureAwait(false);
            }
            else
            {
                eventsToApply = domainEvents;
            }*/

            return await ApplyUpdates(
                    projectionModelContext,
                    eventsToApply,
                    projectionModelEnvelope,
                    cancellationToken)
                .ConfigureAwait(false);
        }

        private async Task<TProjectionModel> GetOrCreateReadModel(
            ProjectionModelEnvelope<TProjectionModel> readModelEnvelope,
            CancellationToken cancellationToken)
        {
            return readModelEnvelope.ProjectionModel
                   ?? await _projectionModelFactory
                       .Create(readModelEnvelope.Id, cancellationToken)
                       .ConfigureAwait(false);
        }
        
        private async Task<ProjectionModelUpdateResult<TProjectionModel>> ApplyUpdates(
            IProjectionModelContext readModelContext,
            IEnumerable<IDomainEvent> domainEvents,
            ProjectionModelEnvelope<TProjectionModel> readModelEnvelope,
            CancellationToken cancellationToken)
        {
            var projectionModel = await GetOrCreateReadModel(readModelEnvelope, cancellationToken);

            _projectionModelDomainEventApplier
                .UpdateProjectionModel(
                    projectionModel,
                    domainEvents,
                    readModelContext);

            var projectionModelVersion = Math.Max(
                domainEvents.Max(e => e.Version),
                readModelEnvelope.Version.GetValueOrDefault());

            return readModelEnvelope.AsModifedResult(projectionModel, projectionModelVersion);
        }
    }
}