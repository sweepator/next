using System;
using System.Threading;
using System.Threading.Tasks;
using Next.Abstractions.EventSourcing;

namespace Next.Cqrs.Commands
{
    public interface ICommandBus
    {
        /// <summary>
        /// Sends a command message
        /// </summary>
        /// <param name="command">command message</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Task</returns>
        Task<Guid> Send<TCommandResponse>(
            ICommand<TCommandResponse>  command,
            CancellationToken cancellationToken = default)
            where TCommandResponse : ICommandResponse;
    }
}