using System.Linq;
using System.Threading;
using Next.Abstractions.Domain.Persistence;
using Next.Application.Pipelines;
using Next.Core;
using Next.Cqrs.Commands;
using Next.HomeBanking.Domain.Aggregates;

namespace Next.HomeBanking.Application.Commands
{
    public class Deposit : AggregateRequestHandler<BankAccountAggregate, BankAccountId, BankAccountState, DepositCommand, DepositCommandResponse>
    {
        public Deposit(
            IAggregateRepository<BankAccountAggregate, BankAccountId, BankAccountState> aggregateRepository) 
            : base(aggregateRepository)
        {
        }
        
        protected override DepositCommandResponse Execute(
            DepositCommand command,
            BankAccountAggregate aggregate,
            IOperationContext context,
            CancellationToken cancellationToken = default)
        {
            var notification = aggregate.Deposit(command.Value);
            return notification.TryGetErrorResponse<DepositCommandResponse>(out var response) ? 
                response : 
                new DepositCommandResponse(
                    aggregate.State.Transactions.Last().Id,
                    aggregate.State.Balance);
        }
    }
}