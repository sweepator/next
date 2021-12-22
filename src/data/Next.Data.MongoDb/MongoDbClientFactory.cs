using System;
using System.Collections.Concurrent;
using MongoDB.Driver;

namespace Next.Data.MongoDb
{
    public class MongoDbClientFactory: IMongoDbClientFactory
    {
        private static readonly ConcurrentDictionary<string, MongoClient> Connections = new();
        
        public MongoClient GetMongoClient(
            string connectionString,
            Action<MongoClient> init = null)
        {
            return Connections.GetOrAdd(connectionString,
                connection =>
                {
                    var settings = MongoClientSettings.FromUrl(new MongoUrl(connectionString));
                    var mongoClient = new MongoClient(settings);
                    init?.Invoke(mongoClient);
                    return mongoClient;
                });
        }
    }
}