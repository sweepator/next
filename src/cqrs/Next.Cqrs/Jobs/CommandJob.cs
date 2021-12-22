using System.Threading.Tasks;
using Next.Abstractions.Jobs;
using Next.Cqrs.Commands;
using Next.Cqrs.Exceptions;

namespace Next.Cqrs.Jobs
{
    public class CommandJob<TCommand, TCommandResponse> : Job<JobRequest<TCommand>>
        where TCommand : class, ICommand<TCommandResponse>
        where TCommandResponse: ICommandResponse
    {
        private readonly ICommandProcessor _commandProcessor;

        public CommandJob(ICommandProcessor commandProcessor)
        {
            _commandProcessor = commandProcessor;
        }
        
        public override async Task Run(JobRequest<TCommand> jobRequest)
        {
            var commandResponse = await _commandProcessor.Execute(jobRequest.Content);

            if (!commandResponse.IsSuccess)
            {
                throw new CommandException(commandResponse.Errors);
            }
        }
    }
}