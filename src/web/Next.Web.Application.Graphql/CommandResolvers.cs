using System.Threading;
using System.Threading.Tasks;
using HotChocolate;
using Next.Cqrs.Commands;

namespace Next.Web.Application.Graphql
{
    public class CommandResolvers<TCommand, TCommandResponse>
        where TCommand : ICommand<TCommandResponse>
        where TCommandResponse : ICommandResponse
    {
        public async Task<TCommandResponse> Execute(
            TCommand input,
            [Service] ICommandProcessor commandProcessor,
            CancellationToken cancellationToken)
        {
            return await commandProcessor.Execute(input, cancellationToken);
        }
    }
}