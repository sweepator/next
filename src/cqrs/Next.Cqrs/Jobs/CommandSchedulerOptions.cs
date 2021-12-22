using System;
using Next.Cqrs.Commands;

namespace Next.Cqrs.Jobs
{
    public class CommandSchedulerOptions<TCommand, TCommandResponse>
        where TCommand : class, ICommand<TCommandResponse>
        where TCommandResponse: ICommandResponse
    {
        public TimeSpan Delay { get; set; } = TimeSpan.FromSeconds(1);
        
        public Func<TCommand, TimeSpan> DelayFunc { get; set; }
    }
}