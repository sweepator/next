namespace Next.EventSourcing.MongoDb
{
    public class MongoDbEventStoreOptions
    {
        public string ConnectionString { get; set; }
        
        public string DataBaseName { get; set; }
        public string CollectionName { get; set; } = "next.events";
    }
}