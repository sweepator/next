using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Next.Abstractions.Bus;

namespace Next.Cqrs.Commands
{
    public class CommandBus : ICommandBus
    {
        private readonly IMessageBus _messageBus;

        public CommandBus(IMessageBus messageBus)
        {
            _messageBus = messageBus;
        }
        
        public async Task<Guid> Send<TCommandResponse>(
            ICommand<TCommandResponse> command, 
            CancellationToken cancellationToken = default) 
            where TCommandResponse : ICommandResponse
        {
            var transactionId = Guid.NewGuid();
            
            var message = BuildMessage(
                transactionId,
                command);
            await _messageBus.Send(message);

            return transactionId;
        }
        
        private static Message BuildMessage<TCommandResponse>(
            Guid transactionId,
            ICommand<TCommandResponse> command)
            where TCommandResponse : ICommandResponse
        {
            var headers = new Dictionary<string, string>
            {
                {
                    Bus.MessageHeaders.TransactionId, transactionId.ToString()
                },
            };

            var message = new Message(
                command,
                command.GetType().Name, 
                headers);
            
            return message;
        }
    }
}