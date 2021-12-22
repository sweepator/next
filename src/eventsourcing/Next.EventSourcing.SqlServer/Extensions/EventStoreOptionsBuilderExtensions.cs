using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Next.Abstractions.EventSourcing.Outbox;
using Next.Abstractions.EventSourcing.Snapshot;
using Next.Data.DbUp.SqlServer;
using Next.Data.SqlServer;
using Next.EventSourcing.SqlServer;

namespace Next.Abstractions.EventSourcing
{
    public static class EventStoreOptionsBuilderExtensions
    {
        public static IEventStoreOptionsBuilder UseSqlServer(
            this IEventStoreOptionsBuilder eventStoreOptionsBuilder,
            string connectionString)
        {
            return eventStoreOptionsBuilder.UseSqlServer(s =>
            {
                s.ConnectionString = connectionString;
            });
        }

        public static IEventStoreOptionsBuilder UseSqlServer(
            this IEventStoreOptionsBuilder eventStoreOptionsBuilder,
            Action<SqlServerEventStoreOptions> setup)
        {
            if (setup == null)
            {
                throw new ArgumentNullException(nameof(setup));
            }

            var name = typeof(SqlServerEventSourcingDataMigrations).Assembly.GetName().Name;

            void SetupAction(SqlServerDataMigrationsOptions o)
            {
                var sqlServerEventStoreOptions = new SqlServerEventStoreOptions();
                setup(sqlServerEventStoreOptions);

                o.ConnectionString = sqlServerEventStoreOptions.ConnectionString;
                o.TimeoutSeconds = sqlServerEventStoreOptions.TimeoutSeconds;
                o.Variables = new Dictionary<string, string>
                {
                    {
                        nameof(sqlServerEventStoreOptions.SchemaName), 
                        sqlServerEventStoreOptions.SchemaName
                    },
                    {
                        nameof(sqlServerEventStoreOptions.EventTableName), 
                        sqlServerEventStoreOptions.EventTableName
                    },
                    {
                        nameof(sqlServerEventStoreOptions.SnapshotTableName), 
                        sqlServerEventStoreOptions.SnapshotTableName
                    }
                };
            }

            eventStoreOptionsBuilder
                .Services
                .Configure(setup)
                .AddOptions<SqlServerDataMigrationsOptions>(name)
                .Configure(SetupAction);
            
            eventStoreOptionsBuilder
                .Services
                .AddTransient<IEventStoreRepository, SqlEventStoreRepository>()
                .AddTransient<ISnapshotRepository, SqlEventStoreRepository>()
                .AddTransient<IOutboxStoreRepository, SqlEventStoreRepository>()
                .AddTransient<ISqlDbContextSession, SqlDbContextSession>()
                .AddSqlServerDataMigrations<SqlServerEventSourcingDataMigrations>(SetupAction);

            return eventStoreOptionsBuilder;
        }
    }
}
