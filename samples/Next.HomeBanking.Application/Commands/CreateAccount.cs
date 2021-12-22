using System.Threading;
using Next.Abstractions.Domain.Persistence;
using Next.Cqrs.Commands;
using Next.Application.Pipelines;
using Next.HomeBanking.Domain.Aggregates;
using Next.Core;

namespace Next.HomeBanking.Application.Commands
{
    public class CreateAccount : CreateAggregateRequestHandler<BankAccountAggregate, BankAccountId, BankAccountState, CreateAccountCommand, CreateAccountCommandResponse>
    {
        public CreateAccount(
            IAggregateRepository<BankAccountAggregate, BankAccountId, BankAccountState> aggregateRepository) 
            : base(aggregateRepository)
        {
        }
        
        protected override CreateAccountCommandResponse Execute(
            CreateAccountCommand command,
            BankAccountAggregate aggregate,
            IOperationContext context,
            CancellationToken cancellationToken = default)
        {
            var notification =  aggregate.Create(
                command.Owner,
                command.Iban,
                true);

            return notification.TryGetErrorResponse<CreateAccountCommandResponse>(out var response) ? 
                response : 
                new CreateAccountCommandResponse(aggregate.Id);
        }
    }
}