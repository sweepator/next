using Next.Cqrs.Commands;
using Next.HomeBanking.Domain.Aggregates;

namespace Next.HomeBanking.Application.Commands
{
    public class DepositCommand : AggregateCommand<BankAccountAggregate,BankAccountId, DepositCommandResponse>
    {
        public decimal Value { get; set; }
    }
}