using System.Threading;
using Next.Abstractions.Domain.Persistence;
using Next.Application.Pipelines;
using Next.Core;
using Next.Cqrs.Commands;
using Next.HomeBanking.Domain.Aggregates;

namespace Next.HomeBanking.Application.Commands
{
    public class CancelAccount : AggregateRequestHandler<BankAccountAggregate, BankAccountId, BankAccountState, CancelAccountCommand, CommandResponse>
    {
        public CancelAccount(
            IAggregateRepository<BankAccountAggregate, BankAccountId, BankAccountState> aggregateRepository) 
            : base(aggregateRepository)
        {
        }
        
        protected override CommandResponse Execute(
            CancelAccountCommand command,
            BankAccountAggregate aggregate,
            IOperationContext context,
            CancellationToken cancellationToken = default)
        {
            var notification =  aggregate.Cancel();

            return notification.TryGetErrorResponse<CommandResponse>(out var response) ? 
                response : 
                CommandResponse.Success();
        }
    }
}