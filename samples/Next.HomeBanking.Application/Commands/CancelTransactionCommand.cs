using Next.Abstractions.Domain;
using Next.Cqrs.Commands;
using Next.HomeBanking.Domain.Aggregates;

namespace Next.HomeBanking.Application.Commands
{
    public class CancelTransactionCommand : AggregateCommand<BankAccountAggregate, BankAccountId, CommandResponse>
    {
        public Id TransactionId { get; set; }
    }
}