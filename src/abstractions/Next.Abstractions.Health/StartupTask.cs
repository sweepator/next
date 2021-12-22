using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Next.Abstractions.Health
{
    public abstract class StartupTask : BackgroundService, IStartupTask
    {
        private const int TaskSleep = 3000;
        private const int TaskSlotSleep = 1000;

        private readonly ILogger _logger;
        private readonly StartupTaskContext _startupTaskContext;
        private bool _isComplete;

        public abstract string Name { get; }

        public bool IsComplete => _isComplete;

        public StartupTask(
            ILogger logger,
            StartupTaskContext startupTaskContext)
        {
            _logger = logger;
            _startupTaskContext = startupTaskContext;
            _startupTaskContext.RegisterTask(this);
        }

        protected abstract Task Work(CancellationToken cancellationToken = default(CancellationToken));

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested &&
                   !IsComplete)
            {
                try
                {
                    while (!_startupTaskContext.GetWorkingTaskName()
                        .Equals(Name, StringComparison.InvariantCultureIgnoreCase))
                    {
                        _logger.Debug("Startup task {TaskName} waiting.", Name);
                        await Task.Delay(TaskSlotSleep, cancellationToken);
                    }

                    _logger.Info("Executing startup task {TaskName}.", Name);

                    await Work(cancellationToken);
                    _isComplete = true;

                    _logger.Info("Startup task {TaskName} completed.", Name);
                }
                catch (Exception e)
                {
                    _logger.Error(e, "Error executing startup task {TaskName}.", Name);
                    await Task.Delay(TaskSleep, cancellationToken);
                }   
            }
        }
    }
}
