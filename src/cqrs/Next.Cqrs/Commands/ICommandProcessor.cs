using System.Threading;
using System.Threading.Tasks;

namespace Next.Cqrs.Commands
{
    public interface ICommandProcessor
    {
        Task<TCommandResponse> Execute<TCommandResponse>(
            ICommand<TCommandResponse>  command,
            CancellationToken cancellationToken = default)
            where TCommandResponse : ICommandResponse;
    }
}