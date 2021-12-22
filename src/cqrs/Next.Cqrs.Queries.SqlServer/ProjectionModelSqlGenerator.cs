using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Options;
using Next.Cqrs.Queries.Projections;
using Next.Application.Cqrs.SqlServer.Extensions;

namespace Next.Cqrs.Queries.SqlServer
{
    public class ProjectionModelSqlGenerator : IProjectionModelSqlGenerator
    {
        private readonly IProjectionModelSqlMetadataProvider _projectionModelSqlMetadataProvider;
        private readonly ProjectionModelSqlGeneratorConfiguration _configuration;
        
        private static readonly ConcurrentDictionary<Type, string> TableNames = new ConcurrentDictionary<Type, string>();
        private static readonly ConcurrentDictionary<Type, IEnumerable<PropertyInfo>> PropertyInfos = new ConcurrentDictionary<Type, IEnumerable<PropertyInfo>>();
        private static readonly ConcurrentDictionary<Type, string> IdentityColumns = new ConcurrentDictionary<Type, string>();
        private static readonly ConcurrentDictionary<Type, string> VersionColumns = new ConcurrentDictionary<Type, string>();

        private readonly ConcurrentDictionary<Type, string> _insertSqls = new ConcurrentDictionary<Type, string>();
        private readonly ConcurrentDictionary<Type, string> _purgeSqls = new ConcurrentDictionary<Type, string>();
        private readonly ConcurrentDictionary<Type, string> _deleteSqls = new ConcurrentDictionary<Type, string>();
        private readonly ConcurrentDictionary<Type, string> _selectSqls = new ConcurrentDictionary<Type, string>();
        private readonly ConcurrentDictionary<Type, string> _selectSqlsById = new ConcurrentDictionary<Type, string>();
        private readonly ConcurrentDictionary<Type, string> _updateSqls = new ConcurrentDictionary<Type, string>();

        public ProjectionModelSqlGenerator(
            IProjectionModelSqlMetadataProvider projectionModelSqlMetadataProvider,
            IOptions<ProjectionModelSqlGeneratorConfiguration> options)
        {
            _projectionModelSqlMetadataProvider = projectionModelSqlMetadataProvider;
            _configuration = options.Value;
        }
        
        public string CreateInsertSql<TProjectionModel>() 
            where TProjectionModel : IProjectionModel
        {
            var projectionModelType = typeof(TProjectionModel);
            if (_insertSqls.TryGetValue(projectionModelType, out var sql))
            {
                return sql;
            }

            var insertColumns = GetInsertColumns<TProjectionModel>().ToList();

            var columnList = insertColumns.SelectToQuotedColumns(
                    _configuration.ColumnQuotedIdentifierPrefix,
                    _configuration.ColumnQuotedIdentifierSuffix)
                .JoinToSql();
            
            var parameterList = insertColumns
                .SelectToSqlParameters()
                .JoinToSql();

            sql = $"INSERT INTO {GetTableName<TProjectionModel>()} ({columnList}) VALUES ({parameterList})";

            _insertSqls[projectionModelType] = sql;

            return sql;
        }

        public string CreateSelectByIdentitySql<TProjectionModel>() 
            where TProjectionModel : IProjectionModel
        {
            var projectionModelType = typeof(TProjectionModel);
            if (_selectSqlsById.TryGetValue(projectionModelType, out var sql))
            {
                return sql;
            }

            var tableName = GetTableName<TProjectionModel>();
            var identityColumn = GetIdentityColumn<TProjectionModel>();

            sql = $"SELECT * FROM {tableName} WHERE {identityColumn} = @ProjectionModelId";

            _selectSqlsById[projectionModelType] = sql;

            return sql;
        }
        
        public string CreateSelectSql<TProjectionModel>() 
            where TProjectionModel : IProjectionModel
        {
            var projectionModelType = typeof(TProjectionModel);
            if (_selectSqls.TryGetValue(projectionModelType, out var sql))
            {
                return sql;
            }

            var tableName = GetTableName<TProjectionModel>();
            sql = $"SELECT * FROM {tableName}";
            _selectSqls[projectionModelType] = sql;

            return sql;
        }
        
        public string CreateDeleteSql<TProjectionModel>()
            where TProjectionModel : IProjectionModel
        {
            var readModelType = typeof(TProjectionModel);
            if (_deleteSqls.TryGetValue(readModelType, out var sql))
            {
                return sql;
            }

            sql = $"DELETE FROM {GetTableName<TProjectionModel>()} WHERE" +
                  $" {_configuration.ColumnQuotedIdentifierPrefix}{GetIdentityColumn<TProjectionModel>()}{_configuration.ColumnQuotedIdentifierSuffix} = @ProjectionModelId";
            _deleteSqls[readModelType] = sql;

            return sql;
        }

        public string CreateUpdateSql<TProjectionModel>() 
            where TProjectionModel : IProjectionModel
        {
            var projectionModelType = typeof(TProjectionModel);
            if (_updateSqls.TryGetValue(projectionModelType, out var sql))
            {
                return sql;
            }

            var identityColumn = GetIdentityColumn<TProjectionModel>();
            var versionColumn = GetVersionColumn<TProjectionModel>();
            var versionCheck = string.IsNullOrEmpty(versionColumn)
                ? string.Empty
                : $"AND {_configuration.ColumnQuotedIdentifierPrefix}{versionColumn}{_configuration.ColumnQuotedIdentifierSuffix} = @_PREVIOUS_VERSION";

            var updateColumns = GetUpdateColumns<TProjectionModel>()
                .SelectToUpdateQuotedColumnsByParameters(
                    _configuration.ColumnQuotedIdentifierPrefix,
                    _configuration.ColumnQuotedIdentifierSuffix)
                .JoinToSql();

            var tableName = GetTableName<TProjectionModel>();

            sql = $"UPDATE {tableName} SET {updateColumns} " +
                  $"WHERE {_configuration.ColumnQuotedIdentifierPrefix}{identityColumn}{_configuration.ColumnQuotedIdentifierSuffix} = @{identityColumn} {versionCheck}";

            _updateSqls[projectionModelType] = sql;

            return sql;
        }

        public string CreatePurgeSql<TProjectionModel>() 
            where TProjectionModel : IProjectionModel
        {
            return _purgeSqls.GetOrAdd(typeof(TProjectionModel), t => $"DELETE FROM {GetTableName(t)}");
        }
        
        private IEnumerable<string> GetInsertColumns<TProjectionModel>() 
            where TProjectionModel : IProjectionModel
        {
            return GetPropertyInfos(typeof(TProjectionModel))
                .Select(p => p.Name);
        }

        private IEnumerable<string> GetUpdateColumns<TProjectionModel>() 
            where TProjectionModel : IProjectionModel
        {
            var identityColumn = GetIdentityColumn<TProjectionModel>();
            return GetInsertColumns<TProjectionModel>()
                .Where(c => c != identityColumn);
        }

        private string GetTableName<TProjectionModel>()
            where TProjectionModel : IProjectionModel
        {
            return GetTableName(typeof(TProjectionModel));
        }

        private string GetTableName(Type projectionModelType)
        {
            return TableNames.GetOrAdd(
                projectionModelType,
                t =>
                {
                    var qip = _configuration.TableQuotedIdentifierPrefix;
                    var qis = _configuration.TableQuotedIdentifierSuffix;

                    var tableInfo = _projectionModelSqlMetadataProvider.GetTableInfo(t);
                    var table = $"Projection-{t.Name.Replace("Projection", string.Empty)}";
                    return string.IsNullOrEmpty(tableInfo?.Schema)
                        ? $"{qip}{table}{qis}"
                        : $"{qip}{tableInfo?.Schema}{qis}.{qip}{table}{qis}";
                });
        }

        private string GetIdentityColumn<TProjectionModel>()
        {
            var projectionModelType = typeof(TProjectionModel);
            
            return IdentityColumns.GetOrAdd(
                projectionModelType,
                t =>
                {
                    var tableInfo = _projectionModelSqlMetadataProvider.GetTableInfo(t);
                    return tableInfo.IdentityColumnName;
                });
        }

        private string GetVersionColumn<TProjectionModel>()
            where TProjectionModel : IProjectionModel
        {
            var projectionModelType = typeof(TProjectionModel);
            
            return VersionColumns.GetOrAdd(
                projectionModelType,
                t =>
                {
                    var tableInfo = _projectionModelSqlMetadataProvider.GetTableInfo(t);
                    return tableInfo.VersionColumnName;
                });
        }

        private IEnumerable<PropertyInfo> GetPropertyInfos(Type projectionModelType)
        {
            return PropertyInfos.GetOrAdd(
                projectionModelType,
                t =>
                {
                    var tableInfo = _projectionModelSqlMetadataProvider.GetTableInfo(t);
                    return t.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                        .Where(p => !tableInfo.IgnoredColumns.Contains(p.Name))
                        .OrderBy(p => p.Name)
                        .ToList();
                });
        }
    }
}