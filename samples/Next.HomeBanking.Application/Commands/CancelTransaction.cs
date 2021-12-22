using System.Threading;
using Next.Abstractions.Domain.Persistence;
using Next.Application.Pipelines;
using Next.Core;
using Next.Cqrs.Commands;
using Next.HomeBanking.Domain.Aggregates;

namespace Next.HomeBanking.Application.Commands
{
    public class CancelTransaction : AggregateRequestHandler<BankAccountAggregate, BankAccountId, BankAccountState, CancelTransactionCommand, CommandResponse>
    {
        public CancelTransaction(
            IAggregateRepository<BankAccountAggregate, BankAccountId, BankAccountState> aggregateRepository) 
            : base(aggregateRepository)
        {
        }
        
        protected override CommandResponse Execute(
            CancelTransactionCommand command,
            BankAccountAggregate aggregate,
            IOperationContext context,
            CancellationToken cancellationToken = default)
        {
            var notification =  aggregate.CancelTransaction(command.TransactionId);

            return notification.TryGetErrorResponse<CommandResponse>(out var response) ? 
                response : 
                CommandResponse.Success();
        }
    }
}