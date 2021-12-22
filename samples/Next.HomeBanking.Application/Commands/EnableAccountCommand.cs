using Next.HomeBanking.Domain.Aggregates;
using Next.Cqrs.Commands;

namespace Next.HomeBanking.Application.Commands
{
    public class EnableAccountCommand : AggregateCommand<BankAccountAggregate,BankAccountId, CommandResponse>
    {
        public bool Enable { get; set; }
    }
}