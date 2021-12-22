using System.Linq;
using System.Threading;
using Next.Abstractions.Domain.Persistence;
using Next.Application.Pipelines;
using Next.Core;
using Next.Cqrs.Commands;
using Next.HomeBanking.Domain.Aggregates;

namespace Next.HomeBanking.Application.Commands
{
    public class Debit : AggregateRequestHandler<BankAccountAggregate, BankAccountId, BankAccountState, DebitCommand, DebitCommandResponse>
    {
        public Debit(
            IAggregateRepository<BankAccountAggregate, BankAccountId, BankAccountState> aggregateRepository) 
            : base(aggregateRepository)
        {
        }
        
        protected override DebitCommandResponse Execute(
            DebitCommand command,
            BankAccountAggregate aggregate,
            IOperationContext context,
            CancellationToken cancellationToken = default)
        {
            var notification = aggregate.Debit(command.Value);
            return notification.TryGetErrorResponse<DebitCommandResponse>(out var response) ? 
                response : 
                new DebitCommandResponse(
                    aggregate.State.Transactions.Last().Id,
                    aggregate.State.Balance);
        }
    }
}