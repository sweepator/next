using Next.Application.Contracts;

namespace Next.Cqrs.Commands
{
    public interface ICommand
    {
    }
    
    public interface ICommand<out TCommandResponse> : ICommand, IRequest<TCommandResponse>
        where TCommandResponse : ICommandResponse
    {
    }
}