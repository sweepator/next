using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Next.Abstractions.Domain;
using Next.Core.Exceptions;
using Next.Cqrs.Queries.Projections;
using Next.Data.SqlServer;

namespace Next.Cqrs.Queries.SqlServer
{
    public class SqlServerQueryStore<TProjectionModel> : QueryStore<TProjectionModel>, ISqlServerQueryStore<TProjectionModel>
        where TProjectionModel : class, IProjectionModel
    {
        private static readonly ConcurrentDictionary<Type, Func<IProjectionModel, int?>> GetVersion = new();
        private static readonly ConcurrentDictionary<Type, Action<IProjectionModel, int?>> SetVersion = new();
        private static readonly ConcurrentDictionary<Type, Action<IProjectionModel, object>> SetIdentity = new();

        private readonly ISqlDbContextSession _sqlDbContextSession;
        private readonly IProjectionModelFactory<TProjectionModel> _projectionModelFactory;
        private readonly IProjectionModelSqlMetadataProvider _projectionModelSqlMetadataProvider;
        private readonly IProjectionModelSqlGenerator _projectionModelSqlGenerator;
        private readonly ILogger<SqlServerQueryStore<TProjectionModel>> _logger;
        private readonly string _connectionString;
        
        public SqlServerQueryStore(
            ISqlDbContextSession sqlDbContextSession,
            ILogger<SqlServerQueryStore<TProjectionModel>> logger,
            IOptions<SqlServerQueryStoreOptions<TProjectionModel>> options, 
            IProjectionModelFactory<TProjectionModel> projectionModelFactory,
            IProjectionModelSqlMetadataProvider projectionModelSqlMetadataProvider,
            IProjectionModelSqlGenerator projectionModelSqlGenerator)
        {
            _sqlDbContextSession = sqlDbContextSession;
            _projectionModelFactory = projectionModelFactory;
            _projectionModelSqlMetadataProvider = projectionModelSqlMetadataProvider;
            _logger = logger;
            _projectionModelSqlGenerator = projectionModelSqlGenerator;
            _connectionString = options.Value.ConnectionString;
        }

        public override async Task<ProjectionModelEnvelope<TProjectionModel>> Get(
            object id, 
            CancellationToken cancellationToken = default)
        { 
            var selectSql = _projectionModelSqlGenerator.CreateSelectByIdentitySql<TProjectionModel>();
            
            using var sqlDbContext = _sqlDbContextSession.GetSqlDbContext(_connectionString);

            var projectionModels = await sqlDbContext
                .QueryAsync<TProjectionModel>(
                    selectSql,
                    new { ProjectionModelId = id })
                .ConfigureAwait(false);

            var projectionModel = projectionModels.SingleOrDefault();
            if (projectionModel == null)
            {
                _logger.Debug("Could not find any sql projection model {TProjectionModel} with id {id}",
                    typeof(TProjectionModel).Name,
                    id);
                return ProjectionModelEnvelope<TProjectionModel>.Empty(id);
            }

            var getVersion = GetVersionFunc();
            var projectionModelVersion = getVersion(projectionModel);

            _logger.Debug("Found sql projection model {TProjectionModel} with version {ProjectionModelVersion}",
                typeof(TProjectionModel).Name,
                projectionModelVersion);

            return ProjectionModelEnvelope<TProjectionModel>.With(
                id, 
                projectionModel, 
                projectionModelVersion);
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
                    cancellationToken: cancellationToken);
            }
        }

        public async Task<IEnumerable<TProjectionModel>> Find(
            DynamicParameters dynamicParameters, 
            string @where, 
            CancellationToken cancellationToken = default)
        {
            var selectSql = _projectionModelSqlGenerator.CreateSelectSql<TProjectionModel>();

            if (!string.IsNullOrWhiteSpace(where))
            {
                selectSql += $" WHERE {@where}";
            }
            
            using var sqlDbContext = _sqlDbContextSession.GetSqlDbContext(_connectionString);
            
            var projectionModels = await sqlDbContext
                .QueryAsync<TProjectionModel>(
                    selectSql,
                    dynamicParameters)
                .ConfigureAwait(false);

            return projectionModels;
        }
        
        public override async Task Delete(
            object id, 
            CancellationToken cancellationToken = default)
        {
            var sql = _projectionModelSqlGenerator.CreateDeleteSql<TProjectionModel>();
            
            using var sqlDbContext = _sqlDbContextSession.GetSqlDbContext(_connectionString);

            var rowsAffected = await sqlDbContext
                .ExecuteAsync(
                    sql,
                    new { ProjectionModelId = id })
                .ConfigureAwait(false);

            if (rowsAffected != 0)
            {
                _logger.Debug("Deleted projection model {id} of type {TProjectionModel}",
                    id,
                    typeof(TProjectionModel).Name);
            }
        }

        public override async Task DeleteAll(CancellationToken cancellationToken = default)
        {
            var sql = _projectionModelSqlGenerator.CreatePurgeSql<TProjectionModel>();
            
            using var sqlDbContext = _sqlDbContextSession.GetSqlDbContext(_connectionString);

            var rowsAffected = await sqlDbContext
                .ExecuteAsync(sql)
                .ConfigureAwait(false);

            _logger.Debug(
                "Purge {rowsAffected} read models of type {1}",
                rowsAffected,
                typeof(TProjectionModel).Name);
        }
        
        private async Task UpdateProjectionModel(
            IProjectionModelContextFactory projectionModelContextFactory,
            Func<IProjectionModelContext, IEnumerable<IDomainEvent>, ProjectionModelEnvelope<TProjectionModel>, CancellationToken, Task<ProjectionModelUpdateResult<TProjectionModel>>> updateProjectionModel,
            ProjectionModelUpdate projectionModelUpdate,
            CancellationToken cancellationToken = default)
        {
            var projectionModelId = projectionModelUpdate.Id;
            var projectionModelEnvelope = await Get(
                    projectionModelId, 
                    cancellationToken)
                .ConfigureAwait(false);
            var projectionModel = projectionModelEnvelope.ProjectionModel;
            var isNew = projectionModel == null;

            if (projectionModel == null)
            {
                projectionModel = await _projectionModelFactory.Create(
                    projectionModelId, 
                    cancellationToken)
                    .ConfigureAwait(false);
                projectionModelEnvelope = ProjectionModelEnvelope<TProjectionModel>.With(
                    projectionModelUpdate.Id, 
                    projectionModel);
            }

            var projectionModelContext = projectionModelContextFactory.Create(projectionModelId, isNew);

            var originalVersion = projectionModelEnvelope.Version;
            var projectionModelUpdateResult = await updateProjectionModel(
                projectionModelContext,
                projectionModelUpdate.DomainEvents,
                projectionModelEnvelope,
                cancellationToken)
                .ConfigureAwait(false);
            
            if (!projectionModelUpdateResult.IsModified)
            {
                return;
            }

            projectionModelEnvelope = projectionModelUpdateResult.Envelope;
            if (projectionModelContext.IsMarkedForDeletion)
            {
                await Delete(
                    projectionModelId, 
                    cancellationToken)
                    .ConfigureAwait(false);
                return;
            }

            var setVersion = SetVersionFunc();
            var setIdentity = SetIdentityFunc();

            setVersion(
                projectionModel, 
                projectionModelEnvelope.Version);
            setIdentity(
                projectionModel, 
                projectionModelEnvelope.Id);

            var sql = isNew
                ? _projectionModelSqlGenerator.CreateInsertSql<TProjectionModel>()
                : _projectionModelSqlGenerator.CreateUpdateSql<TProjectionModel>();

            var dynamicParameters = new DynamicParameters(projectionModel);
            if (originalVersion.HasValue)
            {
                dynamicParameters.Add("_PREVIOUS_VERSION", (int)originalVersion.Value);
            }
            
            using var sqlDbContext = _sqlDbContextSession.GetSqlDbContext(_connectionString);
            
            var rowsAffected = await sqlDbContext
                .ExecuteAsync(
                    sql,
                    dynamicParameters)
                .ConfigureAwait(false);
            
            if (rowsAffected != 1)
            {
                throw new OptimisticConcurrencyException($"Projection model {projectionModelEnvelope.Id} updated by another");
            }

            _logger.Debug("Updated sql server projection model {TProjectionModel} with id {ProjectionModelId} to version {ProjectionModelVersion}",
                typeof(TProjectionModel).Name,
                projectionModelId,
                projectionModelEnvelope.Version);
        }

        private Func<IProjectionModel, int?> GetVersionFunc()
        {
            return GetVersion.GetOrAdd(
                typeof(TProjectionModel),
                t =>
                {
                    var tableInfo = _projectionModelSqlMetadataProvider.GetTableInfo(t);
                    var propertyInfos = typeof(TProjectionModel)
                        .GetProperties(BindingFlags.Instance | BindingFlags.Public);
 
                    var versionPropertyInfo = propertyInfos
                        .SingleOrDefault(p => p.Name.Equals(tableInfo.VersionColumnName, StringComparison.InvariantCultureIgnoreCase));
            
                    if (versionPropertyInfo == null)
                    {
                       return  rm => null as int?;
                    }

                    return rm => (int?) versionPropertyInfo.GetValue(rm);
                });
        }
        
        private Action<IProjectionModel, int?> SetVersionFunc()
        {
            return SetVersion.GetOrAdd(
                typeof(TProjectionModel),
                t =>
                {
                    var tableInfo = _projectionModelSqlMetadataProvider.GetTableInfo(t);
                    var propertyInfos = typeof(TProjectionModel)
                        .GetProperties(BindingFlags.Instance | BindingFlags.Public);
 
                    var versionPropertyInfo = propertyInfos
                        .SingleOrDefault(p => p.Name.Equals(tableInfo.VersionColumnName, StringComparison.InvariantCultureIgnoreCase));
            
                    if (versionPropertyInfo == null)
                    {
                        return (rm, v) => { };
                    }

                    return (rm, v) => versionPropertyInfo.SetValue(rm, v);
                });
        }

        private Action<IProjectionModel, object> SetIdentityFunc()
        {
            return SetIdentity.GetOrAdd(
                typeof(TProjectionModel),
                t =>
                {
                    var tableInfo = _projectionModelSqlMetadataProvider.GetTableInfo(t);
                    var propertyInfos = typeof(TProjectionModel)
                        .GetProperties(BindingFlags.Instance | BindingFlags.Public);
 
                    var identityPropertyInfo = propertyInfos
                        .SingleOrDefault(p => p.Name.Equals(tableInfo.IdentityColumnName, StringComparison.InvariantCultureIgnoreCase));
            
                    if (identityPropertyInfo == null)
                    {
                        return (rm, v) => { };
                    }

                    return (rm, v) => identityPropertyInfo.SetValue(rm, v);
                });
        }
    }
}