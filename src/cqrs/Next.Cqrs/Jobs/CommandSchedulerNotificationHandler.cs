using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Next.Abstractions.Jobs;
using Next.Abstractions.Mapper;
using Next.Application.Contracts;
using Next.Application.Pipelines;
using Next.Cqrs.Commands;

namespace Next.Cqrs.Jobs
{
    public class CommandSchedulerNotificationHandler<TNotification, TCommand, TCommandResponse> : NotificationHandler<TNotification>
        where TNotification : INotification
        where TCommand : class, ICommand<TCommandResponse>
        where TCommandResponse: ICommandResponse
    {
        private readonly ILogger<CommandSchedulerNotificationHandler<TNotification, TCommand, TCommandResponse>> _logger;
        private readonly IOptions<CommandSchedulerOptions<TCommand, TCommandResponse>> _options;
        private readonly IMapper _mapper;
        private readonly IJobService _jobService;

        public CommandSchedulerNotificationHandler(
            ILogger<CommandSchedulerNotificationHandler<TNotification, TCommand, TCommandResponse>> logger,
            IOptions<CommandSchedulerOptions<TCommand, TCommandResponse>> options,
            IMapper mapper,
            IJobService jobService)
        {
            _logger = logger;
            _options = options;
            _mapper = mapper;
            _jobService = jobService;
        }
        
        public override Task Execute(
            TNotification notification,
            IOperationContext context, 
            CancellationToken cancellationToken = default)
        {
            _logger.LogDebug(
                "Scheduling command {Command} with notification {Notification}",
                typeof(TCommand).Name,
                notification);
                
            TCommand command;
            try
            {
                command = _mapper.Map<TCommand>(notification);
                
            }
            catch (Exception e)
            {
                _logger.LogError(e, 
                    "Error mapping command {CommandName} from notification {NotificationName}", 
                    typeof(TCommand).Name,
                    typeof(TNotification).Name);
                throw;
            }
            
            //TODO: setup job context from operation context
            var jobRequest = new JobRequest<TCommand>
            {
                Content = command
            };
            
            var delay = _options.Value.DelayFunc?.Invoke(command) ?? _options.Value.Delay;
            _jobService.Schedule<CommandJob<TCommand, TCommandResponse>, JobRequest<TCommand>>(
                jobRequest, 
                delay);
            
            _logger.LogDebug(
                "Scheduled command {Command} with notification {Notification} for {Date}",
                command,
                notification,
                DateTime.UtcNow.Add(delay));

            return Task.CompletedTask;
        }
    }
}