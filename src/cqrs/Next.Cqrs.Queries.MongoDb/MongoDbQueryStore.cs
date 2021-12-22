using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Next.Abstractions.Domain;
using Next.Core.Exceptions;
using Next.Cqrs.Queries.Projections;
using Next.Data.MongoDb;

namespace Next.Cqrs.Queries.MongoDb
{
    public class MongoDbQueryStore<TProjectionModel> : QueryStore<TProjectionModel>, IMongoDbQueryStore<TProjectionModel>
        where TProjectionModel : class, IProjectionModel
    {
        private readonly ILogger<MongoDbQueryStore<TProjectionModel>> _logger;
        private readonly IMongoDatabase _mongoDatabase;
        private readonly IMongoDbProjectionModelDescriptionProvider _projectionModelDescriptionProvider;

        public MongoDbQueryStore(
            ILogger<MongoDbQueryStore<TProjectionModel>> logger,
            IOptions<MongoDbQueryStoreOptions<TProjectionModel>> options,
            IMongoDbClientFactory mongoDbClientFactory,
            IMongoDbProjectionModelDescriptionProvider projectionModelDescriptionProvider)
        {
            _logger = logger;
            _projectionModelDescriptionProvider = projectionModelDescriptionProvider;
            _mongoDatabase = mongoDbClientFactory
                .GetMongoClient(options.Value.ConnectionString)
                .GetDatabase(options.Value.DatatBase);
        }
        
        public override async Task<ProjectionModelEnvelope<TProjectionModel>> Get(
            object id, 
            CancellationToken cancellationToken = default)
        {
            var modelDescription = _projectionModelDescriptionProvider.GetReadModelDescription<TProjectionModel>();
            
            _logger.LogTrace(
                "Getting projection model {ProjectionModelType} with id {Id} from collection {CollectionName}",
                typeof(TProjectionModel).Name,
                id,
                modelDescription.RootCollectionName.Value);
            
            var collection = _mongoDatabase.GetCollection<TProjectionModel>(modelDescription.RootCollectionName.Value);
            var filter = Builders<TProjectionModel>.Filter.Eq(o => o.Id, id);
            var result = await collection.Find(filter).FirstOrDefaultAsync(cancellationToken);

            return result == null ? 
                ProjectionModelEnvelope<TProjectionModel>.Empty(id) : 
                ProjectionModelEnvelope<TProjectionModel>.With(id, result);
        }

        public override async Task Update(
            IEnumerable<ProjectionModelUpdate> updates, 
            IProjectionModelContextFactory projectionModelContextFactory,
            Func<IProjectionModelContext, IEnumerable<IDomainEvent>, ProjectionModelEnvelope<TProjectionModel>, CancellationToken, Task<ProjectionModelUpdateResult<TProjectionModel>>> updateProjectionModel, 
            CancellationToken cancellationToken = default)
        {
            var modelDescription = _projectionModelDescriptionProvider.GetReadModelDescription<TProjectionModel>();
            
            var ids = updates
                .Select(u => u.Id)
                .Distinct()
                .OrderBy(i => i)
                .ToList();

            _logger.LogTrace(
                "Updating read models of type {ProjectionModelType} with _ids {Ids} in collection {CollectionName}",
                typeof(TProjectionModel).Name,
                string.Join(", ", ids),
                modelDescription.RootCollectionName.Value);

            foreach (var update in updates)
            {
                await UpdateProjectionModel(
                    modelDescription, 
                    update,
                    projectionModelContextFactory,
                    updateProjectionModel,
                    cancellationToken)
                    .ConfigureAwait(false);
            }
        }

        public async Task<IEnumerable<TProjectionModel>> Find(
            Expression<Func<TProjectionModel, bool>> filter, 
            CancellationToken cancellationToken = default)
        {
            var result = await Find(
                filter,
                null,
                cancellationToken);

            return await result.ToListAsync(cancellationToken);
        }

        public async Task Delete(
            Expression<Func<TProjectionModel, bool>> filter, 
            CancellationToken cancellationToken = default)
        {
            var modelDescription = _projectionModelDescriptionProvider.GetReadModelDescription<TProjectionModel>();

            _logger.LogTrace(
                "Deleting {ProjectionModelType} with filter {Filter} from {CollectionName}",
                typeof(TProjectionModel).Name,
                filter?.ToString(),
                modelDescription.RootCollectionName.Value);

            var collection = _mongoDatabase.GetCollection<TProjectionModel>(modelDescription.RootCollectionName.Value);
            await collection.DeleteOneAsync(filter, cancellationToken);
        }

        public override async Task Delete(
            object id, 
            CancellationToken cancellationToken = default)
        {
            var modelDescription = _projectionModelDescriptionProvider.GetReadModelDescription<TProjectionModel>();

            _logger.LogTrace(
                "Deleting {ProjectionModelType} with id {Id} from {CollectionName}",
                typeof(TProjectionModel).Name,
                id,
                modelDescription.RootCollectionName.Value);

            var collection = _mongoDatabase.GetCollection<TProjectionModel>(modelDescription.RootCollectionName.Value);
            await collection.DeleteOneAsync(o => o.Id.Equals(id), cancellationToken);
        }

        public override async Task DeleteAll(CancellationToken cancellationToken = default)
        {
            var modelDescription = _projectionModelDescriptionProvider.GetReadModelDescription<TProjectionModel>();

            _logger.Info(
                "Deleting all {ProjectionModelType} from {CollectionName}",
                typeof(TProjectionModel).Name,
                modelDescription.RootCollectionName.Value);

            await _mongoDatabase.DropCollectionAsync(
                modelDescription.RootCollectionName.Value, 
                cancellationToken);
        }

        public async Task<IAsyncCursor<TProjectionModel>> Find(
            Expression<Func<TProjectionModel, bool>> filter, 
            FindOptions<TProjectionModel, TProjectionModel> options = null, 
            CancellationToken cancellationToken = default)
        {
            var modelDescription = _projectionModelDescriptionProvider.GetReadModelDescription<TProjectionModel>();
            var collection = _mongoDatabase.GetCollection<TProjectionModel>(modelDescription.RootCollectionName.Value);

            _logger.LogTrace(
                "Finding projection model {ProjectionModelType} with filter {Filter} from collection {CollectionName}",
                typeof(TProjectionModel).Name,
                filter?.ToString(),
                collection);
            
            return await collection.FindAsync(
                filter, 
                options, 
                cancellationToken);
        }

        public IQueryable<TProjectionModel> AsQueryable()
        {
            var modelDescription = _projectionModelDescriptionProvider.GetReadModelDescription<TProjectionModel>();
            var collection = _mongoDatabase.GetCollection<TProjectionModel>(modelDescription.RootCollectionName.Value);
            return collection.AsQueryable();
        }
        
        private async Task UpdateProjectionModel(
            ProjectionModelDescription modelDescription,
            ProjectionModelUpdate projectionModelUpdate,
            IProjectionModelContextFactory projectionModelContextFactory,
            Func<IProjectionModelContext, IEnumerable<IDomainEvent>, ProjectionModelEnvelope<TProjectionModel>, CancellationToken, Task<ProjectionModelUpdateResult<TProjectionModel>>> updateProjectionModel,
            CancellationToken cancellationToken)
        {
            var collection = _mongoDatabase.GetCollection<TProjectionModel>(modelDescription.RootCollectionName.Value);
            var filter = Builders<TProjectionModel>.Filter.Where(o => o.Id.Equals(projectionModelUpdate.Id));
            var result = collection.Find(filter).FirstOrDefault();

            var isNew = result == null;

            var projectionModelEnvelope = !isNew
                ? ProjectionModelEnvelope<TProjectionModel>.With(
                    projectionModelUpdate.Id, 
                    result,
                    result.Version)
                : ProjectionModelEnvelope<TProjectionModel>.Empty(projectionModelUpdate.Id);

            var projectionModelContext = projectionModelContextFactory.Create(
                projectionModelUpdate.Id, 
                isNew);
            
            var projectionModelUpdateResult =
                await updateProjectionModel(
                    projectionModelContext, 
                    projectionModelUpdate.DomainEvents, 
                    projectionModelEnvelope,
                    cancellationToken)
                    .ConfigureAwait(false);

            if (!projectionModelUpdateResult.IsModified)
            {
                return;
            }

            if (projectionModelContext.IsMarkedForDeletion)
            {
                await Delete(
                    projectionModelUpdate.Id, 
                    cancellationToken);
                return;
            }

            var originalVersion = projectionModelEnvelope.Version;
            projectionModelEnvelope = projectionModelUpdateResult.Envelope;
            projectionModelEnvelope.ProjectionModel.Version = projectionModelEnvelope.Version;
            
            try
            {
                await collection.ReplaceOneAsync(
                    o => o.Id.Equals(projectionModelUpdate.Id) 
                         && o.Version == originalVersion,
                    projectionModelEnvelope.ProjectionModel,
                    new ReplaceOptions
                    {
                        IsUpsert = true
                    },
                    cancellationToken);
            }
            catch (MongoWriteException e)
            {
                throw new OptimisticConcurrencyException(
                    $"Projection model {projectionModelUpdate.Id} updated by another",
                    e);
            }
        }
    }
}