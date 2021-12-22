using MongoDB.Bson.Serialization.Attributes;

namespace Next.Data.MongoDb
{
    public class MongoDbCounterDataModel
    {
        public string _id { get; set; }

        [BsonElement("seq")]
        public int Seq { get; set; }
    }
}