using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Next.Abstractions.EventSourcing.Snapshot
{
    public class SnapshotProcessorHostingService : IHostedService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ISnapshotProcessor _snapshotProcessor;
        private readonly IOptions<SnapshotProcessorOptions> _options;
        private readonly ILogger<SnapshotProcessorHostingService> _logger;
        private Timer _timer;

        public SnapshotProcessorHostingService(
            IServiceScopeFactory serviceScopeFactory,
            ISnapshotProcessor snapshotProcessor,
            IOptions<SnapshotProcessorOptions> options,
            ILogger<SnapshotProcessorHostingService> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _snapshotProcessor = snapshotProcessor;
            _options = options;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(
                CreateSnapshots,
                null,
                TimeSpan.Zero,
                TimeSpan.FromSeconds(_options.Value.BackgroundLockInSeconds));

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(
                Timeout.Infinite,
                0);

            return Task.CompletedTask;
        }

        private void CreateSnapshots(object state)
        {
            _ = Process();
        }

        private async Task Process()
        {
            _logger.LogTrace("Checking snapshot for processing...");
            
            var snapshot = _snapshotProcessor.GetNextSnapshot();
            if (snapshot == null)
            {
                _logger.LogTrace("No snapshot for process");
                return;
            }

            _logger.LogTrace("Snapshot for process: {AggregateType} with id {Identity} and version {Version}", 
                snapshot.AggregateType.Name,
                snapshot.AggregateIdentity.Value,
                snapshot.AggregateVersion);
            
            using var scope = _serviceScopeFactory.CreateScope();
            var eventStore = scope.ServiceProvider.GetRequiredService<IEventStore>();

            try
            {
                await eventStore.AddSnapshot(snapshot);

                _logger.LogDebug("Snapshot processed: {AggregateType} with id {Identity} and version {Version}", 
                    snapshot.AggregateType.Name,
                    snapshot.AggregateIdentity.Value,
                    snapshot.AggregateVersion);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to process snapshot : {AggregateType} with id {Identity} and version {Version}", 
                    snapshot.AggregateType.Name,
                    snapshot.AggregateIdentity.Value,
                    snapshot.AggregateVersion);
                await Task.Delay(TimeSpan.FromSeconds(_options.Value.BackgroundLockOnErrorInSeconds));
                _snapshotProcessor.AbortSnapshot(snapshot);
            }
        }
    }
}