using Next.HomeBanking.Domain.Aggregates;
using Next.Cqrs.Commands;

namespace Next.HomeBanking.Application.Commands
{
    public class ProcessAccountCommand : AggregateCommand<BankAccountAggregate, BankAccountId, CommandResponse>
    {
    }
}