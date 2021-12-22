using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Next.Abstractions.Bus
{
    public class MessageHostingService: BackgroundService
    {
        private readonly ILogger<MessageHostingService> _logger;
        private readonly IMessageBus _messageBus;
        private const int LockTimeInMilliseconds = 3000;
        
        private bool IsStarted { get; set; }

        public MessageHostingService(
            ILogger<MessageHostingService> logger,
            IMessageBus messageBus)
        {
            _logger = logger;
            _messageBus = messageBus;
        }
        
        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested 
                   && !IsStarted)
            {
                try
                {
                    _logger.LogInformation("Starting up message bus");

                     await _messageBus.Start();
                     IsStarted = true;

                    _logger.LogInformation("Message bus started");
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Error on starting up message bus");
                    await Task.Delay(LockTimeInMilliseconds, cancellationToken);
                }   
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            if (IsStarted)
            {
                await _messageBus.Stop();
            }
        }
    }
}