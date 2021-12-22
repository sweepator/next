using System.Threading;
using System.Threading.Tasks;
using Next.Abstractions.EventSourcing;
using Next.Application.Pipelines;

namespace Next.Cqrs.Commands
{
    internal sealed class CommandProcessor : ICommandProcessor
    {
        private readonly IPipelineEngine _pipelineEngine;

        public CommandProcessor(IPipelineEngine pipelineEngine)
        {
            _pipelineEngine = pipelineEngine;
        }
        
        public async Task<TCommandResponse> Execute<TCommandResponse>(
            ICommand<TCommandResponse> command, 
            CancellationToken cancellationToken = default)
            where TCommandResponse : ICommandResponse
        {
            using var domainTransaction = new DomainTransaction();
            var commandResponse = await _pipelineEngine.Execute(
                command,
                cancellationToken);

            return commandResponse;
        }
    }
}