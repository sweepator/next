using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Next.Abstractions.Health;

namespace Next.Data.MongoDb
{
    public class MongoDbInitializerStartupTask : StartupTask
    {
        private readonly IEnumerable<IMongoDbInitializer> _mongoDbInitializers;
        
        public override string Name => "MongoDbInitializer";
        
        public MongoDbInitializerStartupTask(
            ILogger<MongoDbInitializerStartupTask> logger,
            IEnumerable<IMongoDbInitializer> mongoDbInitializers,
            StartupTaskContext startupTaskContext)
            : base(logger, startupTaskContext)
        {
            _mongoDbInitializers = mongoDbInitializers;
        }
        
        protected override Task Work(CancellationToken cancellationToken = default)
        {
            foreach (var mongoDbInitializer in _mongoDbInitializers)
            {
                mongoDbInitializer.Initialize();
            }

            return Task.CompletedTask;
        }
    }
}