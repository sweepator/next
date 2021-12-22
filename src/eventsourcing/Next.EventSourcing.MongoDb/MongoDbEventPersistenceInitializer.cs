using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Next.Data.MongoDb;

namespace Next.EventSourcing.MongoDb
{
    internal class MongoDbEventPersistenceInitializer : IMongoDbInitializer
    {
        private readonly IMongoDbClientFactory _mongoDbClientFactory;
        private readonly IOptions<MongoDbEventStoreOptions> _eventStoreOptions;
        
        public MongoDbEventPersistenceInitializer(
            IMongoDbClientFactory mongoDbClientFactory,
            IOptions<MongoDbEventStoreOptions> eventStoreOptions)
        {
            _mongoDbClientFactory = mongoDbClientFactory;
            _eventStoreOptions = eventStoreOptions;
        }
        
        public void Initialize()
        {
            var mongoClient = _mongoDbClientFactory.GetMongoClient(_eventStoreOptions.Value.ConnectionString);
            var mongoDatabase= mongoClient.GetDatabase(_eventStoreOptions.Value.DataBaseName);
            var events = mongoDatabase.GetCollection<MongoDbEventModel>(_eventStoreOptions.Value.CollectionName);
            var keys = Builders<MongoDbEventModel>.IndexKeys
                .Ascending(o => o.AggregateId)
                .Ascending(o => o.Version);
            events.Indexes.CreateOne(new CreateIndexModel<MongoDbEventModel>(keys, new CreateIndexOptions {Unique = true}));
        }
    }
}