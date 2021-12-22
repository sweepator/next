using System;
using Microsoft.Extensions.DependencyInjection;
using Next.Data.DbUp.SqlServer;
using Next.EventSourcing.SqlServer;

namespace Next.Abstractions.EventSourcing
{
    public static class EventStoreOptionsBuilderExtensions
    {
        public static IEventStoreOptionsBuilder UseSqlServer(this IEventStoreOptionsBuilder eventStoreOptionsBuilder,
            Action<SqlDataMigrationsOptions> setup)
        {
            if (setup == null)
            {
                throw new ArgumentNullException(nameof(setup));
            }

            eventStoreOptionsBuilder.Services
                .AddTransient<IEventStoreRepository,SqlEventStoreRepository>()
                .Configure(nameof(SqlEventSourcingDataMigrations), setup)
                .AddDataMigrationsStartupTask<SqlEventSourcingDataMigrations>();
            
            return eventStoreOptionsBuilder;
        }
    }
}
