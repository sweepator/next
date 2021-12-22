using System;
using System.Collections.Concurrent;
using System.Linq;

namespace Next.Abstractions.Health
{
    public sealed class StartupTaskContext
    {
        private readonly ConcurrentQueue<IStartupTask> _startupTasks = new();

        public bool IsComplete => _startupTasks.All(o => o.IsComplete);

        public void RegisterTask(IStartupTask startupTask)
        {
            _startupTasks.Enqueue(startupTask);
        }

        public string GetWorkingTaskName()
        {
            var taskName = _startupTasks
                .FirstOrDefault(o => !o.IsComplete)
                ?.Name;

            return taskName;
        }
    }
}
