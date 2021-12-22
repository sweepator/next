using System;
using System.Threading;
using System.Threading.Tasks;
using Next.Abstractions.Domain.Persistence;
using Next.Application.Pipelines;
using Next.HomeBanking.Domain.Aggregates;
using Next.Cqrs.Commands;

namespace Next.HomeBanking.Application.Commands
{
    public class ProcessAccount : AggregateRequestHandlerAsync<BankAccountAggregate, BankAccountId, BankAccountState, ProcessAccountCommand, CommandResponse>
    {
        public ProcessAccount(IAggregateRepository<BankAccountAggregate, BankAccountId, BankAccountState> aggregateRepository) 
            : base(aggregateRepository)
        {
        }

        protected override Task<CommandResponse> Execute(
            ProcessAccountCommand request, 
            BankAccountAggregate aggregate, 
            IOperationContext context,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new CommandResponse());
        }
    }
}