using System;
using MongoDB.Bson.Serialization.Attributes;
using Next.Abstractions.Domain.ValueObjects;

namespace Next.EventSourcing.MongoDb
{
    public class MongoDbEventModel : ValueObject
    {
        [BsonElement("_id")]
        public long _id { get; set; }
        
        public string AggregateId { get; set; }
        
        public string AggregateName { get; set; }
        
        public string Name { get; set; }
        
        public int Version { get; set; }
        
        public DateTime Timestamp { get; set; }
        
        public string Data { get; set; }
        
        public string Metadata { get; set; }
        
        public bool Committed { get; set; }
        
        public Guid TransactionId { get; set; }
        
        public DateTime? CommittedTimestamp { get; set; }
        
    }
}