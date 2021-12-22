using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Next.Abstractions.Domain;
using Next.Abstractions.EventSourcing;

namespace Next.EventSourcing.SqlServer
{
    public class SqlEventStoreRepository : IEventStoreRepository
    {
        public Task Append(string eventStreamId, Guid transactionId, long expectedVersion, IEnumerable<IAggregateEvent> events)
        {
            throw new NotImplementedException();
        }

        public Task Commit(string eventStreamId, Guid transactionId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<IAggregateEvent>> Load(string eventStreamId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<IAggregateEvent>> LoadRange(string eventStreamId, long start, long end)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<IAggregateEvent>> LoadUncommitted(string eventStreamId, Guid transactionId)
        {
            throw new NotImplementedException();
        }


        private static class SqlStatements
        {
            internal const string InsertAggregate =
                        @"INSERT INTO {0}.{1}
                        VALUES
                        (@eventStreamId, @aggregateType, @aggregateEventSeq, @snapshotEventSeq)";

            internal const string InsertEvents =
                @"INSERT INTO {0}.{1}
                VALUES (
                    @aggregateId,
                    @aggregateType,
                    @eventSeq,
                    @timestamp,
                    @msgType,
                    @msgVersion,
                    @msgData,
                    @committed,
                    @transactionId)";

            internal const string GetEventsByStreamId =
                @"SELECT * FROM {0}.{1}
                WHERE aggregate_id = @aggregateId
                ORDER BY event_seq";

            internal const string GetEventsInRangeQuery = "select * from {0}.{1} where aggregate_id = @aggregateId and event_seq >= @start and event_seq <= @end order by event_seq";
            internal const string GetUncommittedEventsQuery = "select * from {0}.{1} where aggregate_id = @aggregateId and transaction_id = @transactionId and committed = 0 order by event_seq";
            internal const string ReadAggregateVersion = "select [aggregate_event_seq] from {0}.{1} where [aggregate_id] = @aggregateId";
            internal const string UpdateAggregateVersion = @"update {0}.{1} set [aggregate_event_seq] = @newVersion where [aggregate_id] = @aggregateId and [aggregate_event_seq] = @expectedVersion";
            internal const string CommitEvents = "update {0}.{1} set committed = 1 where aggregate_Id = @aggregateId and transaction_id = @transactionId";

            internal const string TablesInitialization =
            #region table initialization statement
                @"
			    if not exists(select * from information_schema.schemata where schema_name = '{0}')
			    begin
				    exec('create schema {0} authorization dbo');
			    end

                if not exists (select * from sys.objects where object_id = object_id(N'{0}.{1}')
                                                         and type in (N'U'))
                    begin
                        create table {0}.{1}(
                           [aggregate_id] [nvarchar](256) not null,
                           [aggregate_type] [nvarchar](256) not null,
                           [aggregate_event_seq] [int] not null,
                           [snapshot_event_seq] [int] not null default (0),
                           constraint [PK_Aggregates] primary key clustered
                                ([aggregate_id] asc)
                        )
                    end

                if not exists (select * from sys.objects where object_id = object_id(N'{0}.{2}')
                                                         and type in (N'U'))
                    begin
                        create table {0}.{2}(
                            [aggregate_id] [nvarchar](256) not null,
                            [aggregate_type] [nvarchar](256) not null,
                            [event_seq] [int] not null,
                            [timestamp] [datetime2] not null,
                            [msg_type] [nvarchar](256) not null,
                            [msg_ver] [smallint] not null,
                            [msg_data] [nvarchar](max) not null,
                            [committed] [bit] not null default 0,
                            [transaction_id] UUID not null,
                            constraint [PK_Events] primary key clustered
                                ([aggregate_id] asc, [event_seq] asc)
                        )

                        create nonclustered index [idx_aggregateId_committed]
                            on {0}.{2}([aggregate_id] asc, [committed] desc )

                        create nonclustered index [idx_transationId]
                            on {0}.{2}([transaction_id] asc)
                    end
                ";
            #endregion            
        }
    }
}
