using Next.Cqrs.Commands;
using Next.HomeBanking.Domain.Aggregates;

namespace Next.HomeBanking.Application.Commands
{
    public class DebitCommand : AggregateCommand<BankAccountAggregate, BankAccountId, DebitCommandResponse>
    {
        public decimal Value { get; set; }
    }
}