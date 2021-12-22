using Next.Cqrs.Commands;
using Next.HomeBanking.Domain.Aggregates;

namespace Next.HomeBanking.Application.Commands
{
    public class TransferCommand : AggregateCommand<BankAccountAggregate,BankAccountId, TransferCommandResponse>
    {
        public BankAccountId To { get; set; }
        
        public decimal Value { get; set; }
    }
}