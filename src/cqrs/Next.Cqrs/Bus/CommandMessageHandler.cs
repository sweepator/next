using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Next.Abstractions.Bus;
using Next.Abstractions.EventSourcing;
using Next.Cqrs.Commands;

namespace Next.Cqrs.Bus
{
    internal class CommandMessageHandler<TCommand, TCommandResponse> : IMessageHandler
        where TCommand: ICommand<TCommandResponse>
        where TCommandResponse: ICommandResponse
    {
        private readonly IServiceProvider _serviceProvider;

        public CommandMessageHandler(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        
        public async Task Process(MessageContext messageContext)
        {
            using var scope = _serviceProvider.CreateScope();
            var commandProcessor = scope.ServiceProvider.GetRequiredService<ICommandProcessor>();
            var command = messageContext.GetMessage<TCommand>();
            var transactionId = Guid.Parse(messageContext.Message.Headers[MessageHeaders.TransactionId]);
            
            using var domainTransaction = new DomainTransaction(transactionId);
            await commandProcessor.Execute(command);
        }
    }
}