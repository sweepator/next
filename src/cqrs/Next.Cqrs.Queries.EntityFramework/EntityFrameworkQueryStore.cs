using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging;
using Next.Abstractions.Domain;
using Next.Core.Exceptions;
using Next.Cqrs.Queries.Projections;

namespace Next.Cqrs.Queries.EntityFramework
{
    public class EntityFrameworkQueryStore<TProjectionModel, TDbContext> : QueryStore<TProjectionModel>, IEntityFrameworkQueryStore<TProjectionModel>
        where TProjectionModel : class, IProjectionModel
        where TDbContext: DbContext
    {
        private static readonly ConcurrentDictionary<string, EntityDescriptor> Descriptors = new ConcurrentDictionary<string, EntityDescriptor>();
        
        private readonly ILogger<EntityFrameworkQueryStore<TProjectionModel, TDbContext>> _logger;
        private readonly IProjectionModelFactory<TProjectionModel> _projectionModelFactory;
        private readonly TDbContext _dbContext;

        public EntityFrameworkQueryStore(
            ILogger<EntityFrameworkQueryStore<TProjectionModel, TDbContext>> logger,
            IProjectionModelFactory<TProjectionModel> projectionModelFactory,
            TDbContext dbContext)
        {
            _logger = logger;
            _projectionModelFactory = projectionModelFactory;
            _dbContext = dbContext;
        }
        
        public override async Task<ProjectionModelEnvelope<TProjectionModel>> Get(
            object id, 
            CancellationToken cancellationToken = default)
        {
            var projectionModelType = typeof(TProjectionModel);
            var descriptor = GetDescriptor(_dbContext);
            var entity = await descriptor.Query(
                    _dbContext, 
                    id,
                    false,
                    cancellationToken)
                .ConfigureAwait(false);

            if (entity == null)
            {
                _logger.Debug("Could not find any Entity Framework projection model {TProjectionModel} with id {id}",
                    projectionModelType.Name,
                    id);
                return ProjectionModelEnvelope<TProjectionModel>.Empty(id);
            }

            var entry = _dbContext.Entry(entity);
            var version = descriptor.GetVersion(entry);
            
            _logger.Debug("Found Entity Framework projection model {TProjectionModel} with id {id} and version {version}",
                projectionModelType.Name,
                id,
                version);

            return version.HasValue
                ? ProjectionModelEnvelope<TProjectionModel>.With(
                    id, 
                    entity, 
                    version.Value)
                : ProjectionModelEnvelope<TProjectionModel>.With(
                    id, 
                    entity);
        }

        public override async Task Update(
            IEnumerable<ProjectionModelUpdate> updates, 
            IProjectionModelContextFactory projectionModelContextFactory,
            Func<IProjectionModelContext, IEnumerable<IDomainEvent>, ProjectionModelEnvelope<TProjectionModel>, CancellationToken, Task<ProjectionModelUpdateResult<TProjectionModel>>> updateProjectionModel, 
            CancellationToken cancellationToken = default)
        {
            foreach (var readModelUpdate in updates)
            {
                await UpdateProjectionModel(
                    projectionModelContextFactory,
                    updateProjectionModel,
                    readModelUpdate,
                    cancellationToken);
            }
        }

        public async Task<IEnumerable<TProjectionModel>> Find(
            Expression<Func<TProjectionModel, bool>> filter,
            CancellationToken cancellationToken = default)
        {
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter));
            }
            
            return await _dbContext
                .Set<TProjectionModel>()
                .Where(filter)
                .ToListAsync(cancellationToken);
        }

        public async Task Delete(
            Expression<Func<TProjectionModel, bool>> filter, 
            CancellationToken cancellationToken = default)
        {
            var projectionModelType = typeof(TProjectionModel);
            
            var projectionModels = await _dbContext
                .Set<TProjectionModel>()
                .Where(filter)
                .ToListAsync(cancellationToken);

            _dbContext
                .Set<TProjectionModel>()
                .RemoveRange(projectionModels);
            
            var rowsAffected = await _dbContext
                .SaveChangesAsync(cancellationToken)
                .ConfigureAwait(false);

            if (rowsAffected != 0)
            {
                _logger.Debug("Deleted Entity Framework projection model {TProjectionModel} with filter {id}",
                    projectionModelType.Name,
                    filter?.ToString());
            }
        }

        public override async Task Delete(
            object id, 
            CancellationToken cancellationToken = default)
        {
            var projectionModelType = typeof(TProjectionModel);
            
            var entity = await _dbContext
                .Set<TProjectionModel>()
                .FindAsync(id)
                .ConfigureAwait(false);

            if (entity == null)
            {
                return;
            }
            
            _dbContext.Remove(entity);
            var rowsAffected = await _dbContext
                .SaveChangesAsync(cancellationToken)
                .ConfigureAwait(false);

            if (rowsAffected != 0)
            {
                _logger.Debug("Deleted Entity Framework projection model {TProjectionModel} with id {id}",
                    projectionModelType.Name,
                    id);
            }
        }

        public override async Task DeleteAll(CancellationToken cancellationToken = default)
        {
            var projectionModelType = typeof(TProjectionModel);
            var dbSet = _dbContext
                .Set<TProjectionModel>();
            dbSet.RemoveRange(dbSet);
            
            var rowsAffected = await _dbContext
                .SaveChangesAsync(cancellationToken)
                .ConfigureAwait(false);

            if (rowsAffected != 0)
            {
                _logger.Debug("Deleted all Entity Framework projection model {TProjectionModel}",
                    projectionModelType.Name);
            }
        }
        
        private static EntityDescriptor GetDescriptor(DbContext dbContext)
        {
            return Descriptors.GetOrAdd(dbContext.Database.ProviderName,
                s => new EntityDescriptor(dbContext));
        }
        
        private async Task UpdateProjectionModel(
            IProjectionModelContextFactory projectionModelContextFactory,
            Func<IProjectionModelContext, IEnumerable<IDomainEvent>, ProjectionModelEnvelope<TProjectionModel>, CancellationToken, Task<ProjectionModelUpdateResult<TProjectionModel>>> updateProjectionModel,
            ProjectionModelUpdate projectionModelUpdate,
            CancellationToken cancellationToken)
        {
            var projectionModelType = typeof(TProjectionModel);
            var projectionModelId = projectionModelUpdate.Id;
            var projectionModelEnvelope = await Get(
                    projectionModelId, 
                    cancellationToken)
                .ConfigureAwait(false);

            var entity = projectionModelEnvelope.ProjectionModel;
            var isNew = entity == null;

            if (entity == null)
            {
                entity = await _projectionModelFactory.Create(
                    projectionModelId, 
                    cancellationToken)
                    .ConfigureAwait(false);
                projectionModelEnvelope = ProjectionModelEnvelope<TProjectionModel>.With(
                    projectionModelId, 
                    entity);
            }

            var readModelContext = projectionModelContextFactory.Create(
                projectionModelId, 
                isNew);
            var originalVersion = projectionModelEnvelope.Version;
            var updateResult = await updateProjectionModel(
                    readModelContext,
                    projectionModelUpdate.DomainEvents,
                    projectionModelEnvelope,
                    cancellationToken)
                .ConfigureAwait(false);

            if (!updateResult.IsModified)
                return;

            if (readModelContext.IsMarkedForDeletion)
            {
                await Delete(
                    projectionModelId, 
                    cancellationToken)
                    .ConfigureAwait(false);
                return;
            }

            projectionModelEnvelope = updateResult.Envelope;
            entity = projectionModelEnvelope.ProjectionModel;

            var descriptor = GetDescriptor(_dbContext);
            var entry = isNew
                ? _dbContext.Add(entity)
                : _dbContext.Entry(entity);
            descriptor.SetId(entry, projectionModelId);
            descriptor.SetVersion(entry, originalVersion, projectionModelEnvelope.Version);
            try
            {
                await _dbContext
                    .SaveChangesAsync(cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (DbUpdateConcurrencyException e)
            {
                var databaseValues = await entry
                    .GetDatabaseValuesAsync(cancellationToken)
                    .ConfigureAwait(false);
                entry.CurrentValues.SetValues(databaseValues);
                throw new OptimisticConcurrencyException(e.Message, e);
            }
            
            _logger.Debug("Updated Entity Framework projection model {TProjectionModel} with id {id} to version {version}",
                projectionModelType.Name,
                projectionModelId,
                projectionModelEnvelope.Version);
        }

        private class EntityDescriptor
        {
            private readonly IProperty _key;
            private readonly IProperty _version;
            private readonly Func<DbContext, CancellationToken, object, Task<TProjectionModel>> _queryByIdNoTracking;
            private readonly Func<DbContext, CancellationToken, object, Task<TProjectionModel>> _queryByIdTracking;

            private string Key => _key.Name;
            public string Version => _version.Name;

            public EntityDescriptor(DbContext context)
            {
                var entityType = context.Model.FindEntityType(typeof(TProjectionModel));
                _key = GetKeyProperty(entityType);
                _version = GetVersionProperty(entityType);
                _queryByIdTracking = CompileQueryById(true);
                _queryByIdNoTracking = CompileQueryById(false);
            }



            public Task<TProjectionModel> Query(
                DbContext context,
                object id,
                bool tracking = false,
                CancellationToken cancellationToken = default)
            {
                return tracking
                    ? _queryByIdTracking(context, cancellationToken, id)
                    : _queryByIdNoTracking(context, cancellationToken, id);
            }

            public void SetId(
                EntityEntry entry,
                object id)
            {
                var property = entry.Property(_key.Name);
                property.CurrentValue = id;
            }

            public int? GetVersion(EntityEntry entry)
            {
                if (_version == null) return null;

                var property = entry.Property(_version.Name);
                return (int?) property.CurrentValue;
            }

            public void SetVersion(
                EntityEntry entry,
                int? originalVersion,
                int? currentVersion = null)
            {
                if (_version == null)
                {
                    return;
                }

                var property = entry.Property(_version.Name);
                property.OriginalValue = originalVersion ?? 0;
                property.CurrentValue = currentVersion ?? 0;
            }

            private bool IsConcurrencyProperty(IProperty p)
            {
                return p.IsConcurrencyToken &&
                       (p.ClrType == typeof(long) || p.ClrType == typeof(byte[]));
            }

            private static IProperty GetKeyProperty(IEntityType entityType)
            {
                IProperty key;
                var keyProperties = entityType.FindPrimaryKey() ??
                                    throw new InvalidOperationException("Primary key not found");
                try
                {
                    key = keyProperties.Properties.Single();
                }
                catch (InvalidOperationException e)
                {
                    throw new InvalidOperationException("Read store doesn't support composite primary keys.", e);
                }

                return key;
            }

            private IProperty GetVersionProperty(IEntityType entityType)
            {
                IProperty version;
                var concurrencyProperties = entityType
                    .GetProperties()
                    .Where(IsConcurrencyProperty)
                    .ToList();

                if (concurrencyProperties.Count > 1)
                    concurrencyProperties = concurrencyProperties
                        .Where(p => p.Name.IndexOf("version", StringComparison.OrdinalIgnoreCase) >= 0)
                        .ToList();

                try
                {
                    version = concurrencyProperties.SingleOrDefault();
                }
                catch (InvalidOperationException e)
                {
                    throw new InvalidOperationException("Couldn't determine row version property.", e);
                }

                return version;
            }

            private Func<DbContext, CancellationToken, object, Task<TProjectionModel>> CompileQueryById(bool tracking)
            {
                return tracking
                    ? EF.CompileAsyncQuery((DbContext dbContext, CancellationToken t, object id) =>
                        dbContext
                            .Set<TProjectionModel>()
                            .AsTracking()
                            .SingleOrDefault(e => EF.Property<object>(e, Key) == id))
                    : EF.CompileAsyncQuery((DbContext dbContext, CancellationToken t, object id) =>
                        dbContext
                            .Set<TProjectionModel>()
                            .AsNoTracking()
                            .SingleOrDefault(e => EF.Property<object>(e, Key) == id));
            }
        }
    }
}