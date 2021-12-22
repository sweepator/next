using Next.Cqrs.Commands;
using Next.HomeBanking.Domain.Aggregates;

namespace Next.HomeBanking.Application.Commands
{
    public class CancelAccountCommand : AggregateCommand<BankAccountAggregate,BankAccountId, CommandResponse>
    {
    }
}