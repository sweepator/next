using System.Threading;
using Next.Abstractions.Domain;
using Next.Abstractions.Domain.Persistence;
using Next.Cqrs.Commands;
using Next.Application.Pipelines;
using Next.HomeBanking.Domain.Aggregates;
using Next.Core;

namespace Next.HomeBanking.Application.Commands
{
    public class EnableAccount : AggregateRequestHandler<BankAccountAggregate, BankAccountId, BankAccountState, EnableAccountCommand, CommandResponse>
    {
        public EnableAccount(IAggregateRepository<BankAccountAggregate, BankAccountId, BankAccountState> aggregateRepository) 
            : base(aggregateRepository)
        {
        }

        protected override CommandResponse Execute(
            EnableAccountCommand command,
            BankAccountAggregate aggregate,
            IOperationContext context,
            CancellationToken cancellationToken = default)
        {
            var notification = aggregate.Enable(command.Enable);
            return notification.TryGetErrorResponse(out var response) ? 
                response : 
                CommandResponse.Success();
        }
    }
}