using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Next.Abstractions.Domain;
using Next.Core;
using Next.Cqrs.Queries.Projections;

namespace Next.Cqrs.Queries.Memory
{
    public class InMemoryQueryStore<TProjectionModel> : QueryStore<TProjectionModel>, IInMemoryQueryStore<TProjectionModel>
        where TProjectionModel : class, IProjectionModel
    {
        private readonly AsyncLock _asyncLock = new();
        private readonly Dictionary<object, ProjectionModelEnvelope<TProjectionModel>> _projectionModels = new();
        
        public override Task<ProjectionModelEnvelope<TProjectionModel>> Get(
            object id, 
            CancellationToken cancellationToken = default)
        {
            var projectionModelEnvelope = _projectionModels.TryGetValue(id, out var readModelEnvelope)
                ? readModelEnvelope
                : ProjectionModelEnvelope<TProjectionModel>.Empty(id);

            return Task.FromResult(projectionModelEnvelope);
        }

        public override async Task Update(
            IEnumerable<ProjectionModelUpdate> updates, 
            IProjectionModelContextFactory projectionModelContextFactory,
            Func<IProjectionModelContext, IEnumerable<IDomainEvent>, ProjectionModelEnvelope<TProjectionModel>, CancellationToken, Task<ProjectionModelUpdateResult<TProjectionModel>>> updateProjectionModel, 
            CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.WaitAsync(cancellationToken).ConfigureAwait(false))
            {
                foreach (var projectionModelUpdate in updates)
                {
                    var projectionModelId = projectionModelUpdate.Id;

                    var isNew = !_projectionModels.TryGetValue(projectionModelId, out var readModelEnvelope);

                    if (isNew)
                    {
                        readModelEnvelope = ProjectionModelEnvelope<TProjectionModel>.Empty(projectionModelId);
                    }

                    var readModelContext = projectionModelContextFactory.Create(projectionModelId, isNew);

                    var readModelUpdateResult = await updateProjectionModel(
                            readModelContext,
                            projectionModelUpdate.DomainEvents,
                            readModelEnvelope,
                            cancellationToken)
                        .ConfigureAwait(false);
                    if (!readModelUpdateResult.IsModified)
                    {
                        return;
                    }
                    
                    readModelEnvelope = readModelUpdateResult.Envelope;

                    if (readModelContext.IsMarkedForDeletion)
                    {
                        _projectionModels.Remove(projectionModelId);
                    }
                    else
                    {
                        _projectionModels[projectionModelId] = readModelEnvelope;
                    }
                }
            }
        }
        
        public Task<IEnumerable<TProjectionModel>> Find(
            Expression<Func<TProjectionModel, bool>> filter,
            CancellationToken cancellationToken = default)
        {
            var result = _projectionModels
                .Values
                .Select(e => e.ProjectionModel)
                .AsQueryable()
                .Where(filter)
                .ToList();

            return Task.FromResult<IEnumerable<TProjectionModel>>(result);
        }

        public async Task Delete(
            Expression<Func<TProjectionModel, bool>> filter, 
            CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.WaitAsync(cancellationToken).ConfigureAwait(false))
            {
                var projectionModels = _projectionModels
                    .Values
                    .Select(e => e.ProjectionModel)
                    .AsQueryable()
                    .Where(filter)
                    .ToList();

                foreach (var projectionModel in projectionModels)
                {
                    _projectionModels.Remove(projectionModel.Id);
                }
            }
        }

        public override async Task Delete(
            object id, 
            CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.WaitAsync(cancellationToken).ConfigureAwait(false))
            {
                _projectionModels.Remove(id);
            }
        }

        public override async Task DeleteAll(CancellationToken cancellationToken = default)
        {
            using (await _asyncLock.WaitAsync(cancellationToken).ConfigureAwait(false))
            {
                _projectionModels.Clear();
            }
        }
    }
}