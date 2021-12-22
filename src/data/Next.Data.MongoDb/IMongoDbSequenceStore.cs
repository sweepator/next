namespace Next.Data.MongoDb
{
    public interface IMongoDbSequenceStore
    {
        long GetNextSequence(string name);
    }
}