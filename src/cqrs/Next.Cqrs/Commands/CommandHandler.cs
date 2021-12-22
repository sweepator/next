using Next.Application.Pipelines;

namespace Next.Cqrs.Commands
{
    public abstract class CommandHandler<TCommand, TCommandResponse> : RequestHandler<TCommand, TCommandResponse>
        where TCommand : ICommand<TCommandResponse>
        where TCommandResponse : ICommandResponse
    {
    }
}