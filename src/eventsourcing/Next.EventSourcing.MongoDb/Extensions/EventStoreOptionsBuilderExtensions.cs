using System;
using DnsClient.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Next.Abstractions.EventSourcing.Outbox;
using Next.Data.MongoDb;
using Next.EventSourcing.MongoDb;

namespace Next.Abstractions.EventSourcing
{
    public static class EventStoreOptionsBuilderExtensions
    {
        public static IEventStoreOptionsBuilder UseMongoDb(
            this IEventStoreOptionsBuilder eventStoreOptionsBuilder,
            Action<MongoDbEventStoreOptions> setup,
            Action<MongoClient> init = null)
        {
            if (setup == null)
            {
                throw new ArgumentNullException(nameof(setup));
            }

            eventStoreOptionsBuilder
                .Services
                .Configure(setup);

            eventStoreOptionsBuilder
                .Services
                .TryAddSingleton<IMongoDbClientFactory, MongoDbClientFactory>();
            
            eventStoreOptionsBuilder
                .Services
                .AddMongoDbInitializerStartupTask();

            MongoDbEventStoreRepository SetupRepository(IServiceProvider sp)
            {
                var eventStoreOptions = sp.GetRequiredService<IOptions<MongoDbEventStoreOptions>>();
                var eventPublisherOptions = sp.GetRequiredService<IOptions<EventPublisherOptions>>();
                var mongoDbClientFactory = sp.GetRequiredService<IMongoDbClientFactory>();
                var mongoClient = mongoDbClientFactory.GetMongoClient(
                    eventStoreOptions.Value.ConnectionString, 
                    init);
                var mongoDataBase = mongoClient.GetDatabase(eventStoreOptions.Value.DataBaseName);

                return new MongoDbEventStoreRepository(
                    sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<MongoDbEventStoreRepository>>(), 
                    mongoDataBase, 
                    new MongoDbSequenceStore(mongoDataBase), 
                    eventPublisherOptions,
                    eventStoreOptions);
            }

            eventStoreOptionsBuilder
                .Services   
                .AddTransient<IOutboxStoreRepository>(SetupRepository)
                .AddTransient<IEventStoreRepository>(SetupRepository)
                .AddSingleton<IMongoDbInitializer, MongoDbEventPersistenceInitializer>();

            return eventStoreOptionsBuilder;
        }
    }
}
