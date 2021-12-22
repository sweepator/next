using MongoDB.Driver;

namespace Next.Data.MongoDb
{
    public class MongoDbSequenceStore : IMongoDbSequenceStore
    {
        private const string CollectionName = "next.counter";
        
        private readonly IMongoDatabase _mongoDatabase;

        public MongoDbSequenceStore(IMongoDatabase mongoDatabase)
        {
            _mongoDatabase = mongoDatabase;
        }

        public long GetNextSequence(string name)
        {
            var model = _mongoDatabase.GetCollection<MongoDbCounterDataModel>(CollectionName)
                .FindOneAndUpdate<MongoDbCounterDataModel>(
                    o => o._id == name,
                    new UpdateDefinitionBuilder<MongoDbCounterDataModel>()
                        .Inc(
                            o => o.Seq,
                            1),
                    new FindOneAndUpdateOptions<MongoDbCounterDataModel>
                    {
                        IsUpsert = true,
                        ReturnDocument = ReturnDocument.After
                    });

            return model.Seq;
        }
    }
}