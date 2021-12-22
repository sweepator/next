using System;
using MongoDB.Driver;

namespace Next.Data.MongoDb
{
    public interface IMongoDbClientFactory
    {
        MongoClient GetMongoClient(
            string connectionString,
            Action<MongoClient> init = null);
    }
}