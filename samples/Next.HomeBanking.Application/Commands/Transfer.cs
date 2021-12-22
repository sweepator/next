using System.Linq;
using System.Threading;
using Next.Abstractions.Domain.Persistence;
using Next.Application.Pipelines;
using Next.Core;
using Next.Cqrs.Commands;
using Next.HomeBanking.Domain.Aggregates;

namespace Next.HomeBanking.Application.Commands
{
    public class Transfer : AggregateRequestHandler<BankAccountAggregate, BankAccountId, BankAccountState, TransferCommand, TransferCommandResponse>
    {
        public Transfer(
            IAggregateRepository<BankAccountAggregate, BankAccountId, BankAccountState> aggregateRepository) 
            : base(aggregateRepository)
        {
        }
        
        protected override TransferCommandResponse Execute(
            TransferCommand command,
            BankAccountAggregate aggregate,
            IOperationContext context,
            CancellationToken cancellationToken = default)
        {
            var notification = aggregate.StartTransaction(
                command.To,
                command.Value);
            return notification.TryGetErrorResponse<TransferCommandResponse>(out var response) ? 
                response : 
                new TransferCommandResponse(
                    aggregate.State.Transactions.Last().Id,
                    aggregate.State.Balance);
        }
    }
}