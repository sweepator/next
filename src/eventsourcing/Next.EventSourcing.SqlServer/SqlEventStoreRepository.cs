using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Next.Abstractions.Data;
using Next.Abstractions.Domain;
using Next.Abstractions.Domain.Exceptions;
using Next.Abstractions.EventSourcing;
using Next.Abstractions.EventSourcing.Outbox;
using Next.Abstractions.EventSourcing.Snapshot;
using Next.Data.SqlServer;

namespace Next.EventSourcing.SqlServer
{
    public class SqlEventStoreRepository : IEventStoreRepository, IOutboxStoreRepository, ISnapshotRepository
    {
        private readonly ISqlDbContextSession _sqlDbContextSession;
        private readonly SqlServerEventStoreOptions _options;

        public SqlEventStoreRepository(
            ISqlDbContextSession sqlDbContextSession,
            IOptions<SqlServerEventStoreOptions> options)
        {
            _sqlDbContextSession = sqlDbContextSession;
            _options = options.Value;
        }
        
        public async Task<IEnumerable<ISerializedEvent>> Get(IIdentity id)
        {
            var query = string.Format(
                SqlStatements.GetEventsByStreamId,
                _options.SchemaName,
                _options.EventTableName);

            var args = new { aggregateId = id.Value };
            return await Load(
                query,
                args);
        }
        
        public async Task<ISerializedSnapshot> GetSnapshot(IIdentity id)
        {
            var statement = string.Format(
                SqlStatements.ReadSnapshot,
                _options.SchemaName,
                _options.SnapshotTableName);
            
            var args = new { aggregateId = id.Value };
            
            using var sqlDbContext = _sqlDbContextSession.GetSqlDbContext(_options.ConnectionString);
            
            var result = await sqlDbContext
                .QueryAsync<SnapshotEntity>(statement, args)
                .ConfigureAwait(false);

            return result
                .Select(o => new SerializedSnapshot(
                    o.aggregate_id,
                    o.aggregate_name,
                    o.name,
                    o.aggregate_version,
                    o.timestamp,
                    o.data,
                    o.metadata))
                .SingleOrDefault();
        }
        
        public async Task Save(
            IIdentity id, 
            Guid transactionId, 
            long expectedVersion, 
            IEnumerable<ISerializedEvent> events,
            Func<Task> afterSaveCallback)
        {
            if (!events.Any())
            {
                return;
            }
            
            var sqlExceptionThrown = false;

            try
            {
                using var sqlDbContext = _sqlDbContextSession.GetSqlDbContext(_options.ConnectionString);

                await sqlDbContext.BeginTransaction();

                await InsertEvents(
                        sqlDbContext,
                        id.Value,
                        transactionId,
                        events)
                    .ConfigureAwait(false);

                await afterSaveCallback();
                
                await sqlDbContext.CommitTransaction();
            }
            catch (SqlException e)
            {                    
                if (e.Number == SqlErrors.DuplicateKeyError || e.Number == SqlErrors.UniqueConstraintError)
                {
                    sqlExceptionThrown = true;
                }
                else
                {
                    throw;
                }
            }

            if (sqlExceptionThrown)
            {
                var curVersion = await GetAggregateVersion(id.Value)
                    .ConfigureAwait(false);
                throw new ConcurrencyException(
                    id.Value, 
                    expectedVersion, 
                    curVersion);
            }
        }

        public async Task SaveSnapshot(ISerializedSnapshot snapshot)
        {
            var statement = string.Format(
                SqlStatements.InsertSnapshot,
                _options.SchemaName,
                _options.SnapshotTableName);
            
            using var sqlDbContext = _sqlDbContextSession.GetSqlDbContext(_options.ConnectionString);
            await sqlDbContext
                .ExecuteAsync(
                    statement,
                    new
                    {
                        aggregateId = snapshot.AggregateId,
                        aggregateName = snapshot.AggregateName,
                        name = snapshot.Name,
                        version = snapshot.Version,
                        timestamp = snapshot.Timestamp,
                        data = snapshot.Data,
                        metaData = snapshot.Metadata
                    })
                .ConfigureAwait(false);
        }
        
        public async Task Commit(
            IIdentity id, 
            Guid transactionId)
        {
            using var sqlDbContext = _sqlDbContextSession.GetSqlDbContext(_options.ConnectionString);

            var cmd = string.Format(
                SqlStatements.CommitEvents,
                _options.SchemaName,
                _options.EventTableName);

            await sqlDbContext.ExecuteAsync(cmd, new
                {
                    aggregateId = id.Value,
                    transactionId,
                    timestamp = DateTime.UtcNow
                })
                .ConfigureAwait(false);
        }

        public async Task<IPagedList<ISerializedEvent>> GetCommitted(PageSelection pageSelection)
        {
            var query = string.Format(
                SqlStatements.GetPaginatedEventsQuery,
                _options.SchemaName,
                _options.EventTableName);

            var args = new
            {
                offset = (pageSelection.Number - 1) * pageSelection.Size, 
                size = pageSelection.Size
            };

            var events= await Load(
                query,
                args);

            return new PagedList<ISerializedEvent>(
                events.AsQueryable(), 
                pageSelection.Number, 
                pageSelection.Size);
        }

        public async Task<IEnumerable<ISerializedEvent>> GetRange(
            IIdentity id, 
            int start, 
            int? end = null)
        {
            var query =
                end.HasValue
                    ? string.Format(
                        SqlStatements.GetEventsInRangeQuery,
                        _options.SchemaName,
                        _options.EventTableName)
                    : string.Format(
                        SqlStatements.GetEventsFromQuery,
                        _options.SchemaName,
                        _options.EventTableName);

            object arguments;

            if (end.HasValue)
            {
                arguments = new
                {
                    aggregateId = id.Value,
                    start,
                    end
                };
            }
            else
            {
                arguments = new
                {
                    aggregateId = id.Value,
                    start
                };
            }

            return await Load(
                query, 
                arguments);
        }
        
        public async Task<IEnumerable<ISerializedEvent>> GetAllUnCommitted(int limit)
        {
            var query = string.Format(
                SqlStatements.GetAllUncommittedEventsQuery,
                _options.SchemaName,
                _options.EventTableName);

            return await Load(
                query,
                new {limit});
        }

        public async Task<IEnumerable<ISerializedEvent>> GetUncommitted(IIdentity id)
        {
            var query = string.Format(
                SqlStatements.GetUncommittedEventsQuery,
                _options.SchemaName,
                _options.EventTableName);

            return await Load(
                query,
                new
                {
                    aggregateId = id.Value
                });
        }
        
        private async Task InsertEvents(
            ISqlDbContext sqlDbContext,
            string eventStreamId, 
            Guid transactionId, 
            IEnumerable<ISerializedEvent> events)
        {
            var sqlEvents = events.Select(e => new
            {
                aggregateId = eventStreamId,
                aggregateName = e.AggregateName,
                name = e.EventName,
                version = e.Version,
                timestamp = e.Timestamp,
                data = e.Data,
                metaData = e.Metadata,
                committed = 0,
                transactionId,
                committed_timestamp = (DateTime?)null
            });
            
            var statement = string.Format(
                SqlStatements.InsertEvents,
                _options.SchemaName,
                _options.EventTableName);
            
            await sqlDbContext
                .ExecuteAsync(
                    statement, 
                    sqlEvents)
                .ConfigureAwait(false);
        }

        private async Task<int> GetAggregateVersion(string eventStreamId)
        {
            var statement = string.Format(
                SqlStatements.ReadAggregateVersion,
                _options.SchemaName,
                _options.EventTableName);
            
            var args = new { aggregateId = eventStreamId };
            
            using var sqlDbContext = _sqlDbContextSession.GetSqlDbContext(_options.ConnectionString);
            
            var res = await sqlDbContext
                .QueryAsync<int>(statement, args)
                .ConfigureAwait(false);
            
            return res.Single();
        }

        private async Task<IEnumerable<ISerializedEvent>> Load(string query, object args)
        { 
            using var sqlDbContext = _sqlDbContextSession.GetSqlDbContext(_options.ConnectionString);

            var result = await sqlDbContext
                .QueryAsync<EventEntity>(query, args)
                .ConfigureAwait(false);

            return result.Select(e => new SerializedEvent(
                e.aggregate_id,
                e.aggregate_name,
                e.name,
                e.version,
                e.timestamp,
                e.data,
                e.metadata,
                e.transaction_id,
                e.committed));
        }

        private async Task<PagedList<ISerializedEvent>> LoadPage(PageSelection pageSelection)
        { 
            using var sqlDbContext = _sqlDbContextSession.GetSqlDbContext(_options.ConnectionString);

            var result = await sqlDbContext
                .QueryAsync<EventEntity>(SqlStatements.GetPaginatedEventsQuery, new{})
                .ConfigureAwait(false);

            var events = result.Select(e => new SerializedEvent(
                e.aggregate_id,
                e.aggregate_name,
                e.name,
                e.version,
                e.timestamp,
                e.data,
                e.metadata,
                e.transaction_id,
                e.committed));

            return new PagedList<ISerializedEvent>(
                events.AsQueryable(),
                pageSelection.Number,
                pageSelection.Size,
                result.Any() ? result.FirstOrDefault().total_events : 0);
        }

        private class SqlErrors
        {
            internal const int DuplicateKeyError = 2601;
            internal const int UniqueConstraintError = 2627;
        }
        
        private static class SqlStatements
        {
            internal const string InsertEvents =
                @"INSERT INTO {0}.{1} VALUES (@aggregateId,@aggregateName,@name,@version,@timestamp,@data,@metaData,@committed,@transactionId,@committed_timestamp)";
            
            internal const string InsertSnapshot =
                @"INSERT INTO {0}.{1} VALUES (@aggregateId,@aggregateName,@version,@name,@data,@metaData,@timestamp)";
            
            internal const string GetEventsByStreamId =
                @"SELECT * FROM {0}.{1} WHERE aggregate_id = @aggregateId ORDER BY version";
            
            internal const string ReadAggregateVersion = "SELECT [version] FROM {0}.{1} WHERE [aggregate_id] = @aggregateId";
            
            internal const string ReadSnapshot = "SELECT TOP(1) * FROM {0}.{1} WHERE [aggregate_id] = @aggregateId ORDER BY [aggregate_version] DESC";
            
            internal const string CommitEvents = "UPDATE {0}.{1} SET [committed] = 1, committed_timestamp = @timestamp  WHERE aggregate_Id = @aggregateId AND transaction_id = @transactionId and [committed] = 0";
            
            internal const string GetEventsInRangeQuery = "SELECT * FROM {0}.{1} WHERE aggregate_id = @aggregateId AND version >= @start AND version <= @end ORDER BY version";

            internal const string GetEventsFromQuery = "SELECT * FROM {0}.{1} WHERE aggregate_id = @aggregateId AND version >= @start ORDER BY version";

            internal const string GetPaginatedEventsQuery = @"WITH Data_Events
                            AS
                            (
                                SELECT *
                                FROM {0}.{1}
                            ), 
                            Count_Events
                            AS 
                            (
                                SELECT COUNT(*) AS total_events FROM Data_Events
                            )
                            SELECT *
                            FROM Data_Events
                            CROSS JOIN Count_Events
                            ORDER BY timestamp
                            OFFSET @offset ROWS
                            FETCH NEXT @size ROWS ONLY;";

            internal const string InsertAggregate =
                        @"INSERT INTO {0}.{1}
                        VALUES
                        (@eventStreamId, @aggregateType, @aggregateEventSeq, @snapshotEventSeq)";
            
            internal const string GetUncommittedEventsQuery = "select * from {0}.{1} where aggregate_id = @aggregateId and [committed] = 0 order by version";
            
            internal const string GetAllUncommittedEventsQuery = "select TOP(@limit) * from {0}.{1} where [committed] = 0 order by version";
            
            internal const string UpdateAggregateVersion = @"update {0}.{1} set [aggregate_event_seq] = @newVersion where [aggregate_id] = @aggregateId and [aggregate_event_seq] = @expectedVersion";
        }
    }
}
