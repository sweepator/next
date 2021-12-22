using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Next.Abstractions.Data;
using Next.Abstractions.Domain;
using Next.Abstractions.Domain.Exceptions;
using Next.Abstractions.EventSourcing;
using Next.Abstractions.EventSourcing.Outbox;
using Next.Abstractions.EventSourcing.Snapshot;
using Next.Data.MongoDb;

namespace Next.EventSourcing.MongoDb
{
    public class MongoDbEventStoreRepository : IEventStoreRepository, IOutboxStoreRepository, ISnapshotRepository
    {
        private readonly ILogger<MongoDbEventStoreRepository> _logger;
        private readonly IMongoDatabase _mongoDatabase;
        private readonly IMongoDbSequenceStore _mongoDbSequenceStore;
        private readonly IOptions<MongoDbEventStoreOptions> _eventStoreOptions;
        private IMongoCollection<MongoDbEventModel> MongoDbEventStoreCollection => _mongoDatabase.GetCollection<MongoDbEventModel>(_eventStoreOptions.Value.CollectionName);
        
        public MongoDbEventStoreRepository(
            ILogger<MongoDbEventStoreRepository> logger,
            IMongoDatabase mongoDatabase,
            IMongoDbSequenceStore mongoDbSequenceStore,
            IOptions<EventPublisherOptions> eventPublisherOptions,
            IOptions<MongoDbEventStoreOptions> eventStoreOptions)
        {
            _logger = logger;
            _mongoDatabase = mongoDatabase;
            _mongoDbSequenceStore = mongoDbSequenceStore;
            _eventStoreOptions = eventStoreOptions;
        }
        
        public async Task<IEnumerable<ISerializedEvent>> Get(IIdentity id)
        {
            var result = await MongoDbEventStoreCollection
                .Find(model => model.AggregateId == id.Value)
                .ToListAsync()
                .ConfigureAwait( false);

            return Load(result);
        }

        public Task<ISerializedSnapshot> GetSnapshot(IIdentity id)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<ISerializedEvent>> GetRange(
            IIdentity id, 
            int start, 
            int? end = null)
        {
            var result = await MongoDbEventStoreCollection
                .Find(model => model.AggregateId == id.Value 
                               && model.Version >= start &&
                               (!end.HasValue || model.Version <= end.GetValueOrDefault()))
                .ToListAsync()
                .ConfigureAwait(false);

            return Load(result);
        }

        public async Task<IEnumerable<ISerializedEvent>> GetUncommitted(IIdentity id)
        {
            var result = await MongoDbEventStoreCollection
                .Find(model => model.AggregateId== id.Value && model.Committed == false)
                .ToListAsync()
                .ConfigureAwait( false);

            return Load(result);
        }

        public async Task Save(
            IIdentity id, 
            Guid transactionId, 
            long expectedVersion, 
            IEnumerable<ISerializedEvent> events,
            Func<Task> afterSaveCallback)
        {
            var eventDataModels = events
                .Select(e => new MongoDbEventModel()
                {
                    _id = _mongoDbSequenceStore.GetNextSequence(_eventStoreOptions.Value.CollectionName),
                    AggregateId = id.Value,
                    AggregateName = e.AggregateName,
                    Name = e.EventName,
                    Version = e.Version,
                    Timestamp = e.Timestamp,
                    Data = e.Data,
                    Metadata = e.Metadata,
                    Committed = false,
                    TransactionId = transactionId,
                    CommittedTimestamp = null
                })
                .OrderBy(o => o.Version)
                .ToList();

            _logger.LogDebug(
                "Committing {EventsCount} events to MongoDb event store for entity with id {Id}",
                eventDataModels.Count,
                id.Value);
            try
            {
                await MongoDbEventStoreCollection
                    .InsertManyAsync(eventDataModels)
                    .ConfigureAwait(false);
                
                await afterSaveCallback();
            }
            catch (MongoBulkWriteException)
            {
                var curVersion = await GetAggregateVersion(id.Value)
                    .ConfigureAwait(false);
                
                throw new ConcurrencyException(
                    id.Value, 
                    expectedVersion, 
                    curVersion);
            }
        }

        public Task SaveSnapshot(ISerializedSnapshot snapshot)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<ISerializedEvent>> GetAllUnCommitted(int limit)
        {
            var result = await MongoDbEventStoreCollection
                .Find(o => o.Committed == false)
                .Limit(limit)
                .ToListAsync();

            return Load(result);
        }

        public async Task Commit(
            IIdentity id, 
            Guid transactionId)
        {
            await MongoDbEventStoreCollection
                .UpdateManyAsync(
                    o => o.AggregateId == id.Value,
                    Builders<MongoDbEventModel>.Update
                        .Set(u => u.Committed, true)
                        .Set(u => u.CommittedTimestamp, DateTime.UtcNow),
                    new UpdateOptions {IsUpsert = false})
                .ConfigureAwait(false);
        }

        public async Task<IPagedList<ISerializedEvent>> GetCommitted(PageSelection pageSelection)
        {
            var result = await MongoDbEventStoreCollection
                .Find(o => o.Committed)
                .Skip((pageSelection.Number - 1) * pageSelection.Size)
                .Limit(pageSelection.Size)
                .ToListAsync();

            var count = await MongoDbEventStoreCollection.CountDocumentsAsync(o => o.Committed);
            
            return new PagedList<ISerializedEvent>(
                Load(result), 
                pageSelection.Number, 
                pageSelection.Size,
                count);
        }

        private IEnumerable<ISerializedEvent> Load(IEnumerable<MongoDbEventModel> mongoDbEventModels)
        {
            return mongoDbEventModels.Select(e => new SerializedEvent(
                e.AggregateId,
                e.AggregateName,
                e.Name,
                e.Version,
                e.Timestamp,
                e.Data,
                e.Metadata,
                e.TransactionId,
                e.Committed));
        }
        
        private async Task<long> GetAggregateVersion(string eventStreamId)
        {
            var result = await MongoDbEventStoreCollection
                .Find(model => model.AggregateId == eventStreamId)
                .SingleAsync()
                .ConfigureAwait( false);
            return result.Version;
        }
    }
}