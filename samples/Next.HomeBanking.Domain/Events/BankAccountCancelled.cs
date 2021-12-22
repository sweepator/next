using Next.Abstractions.Domain;
using Next.HomeBanking.Domain.Aggregates;

namespace Next.HomeBanking.Domain.Events
{
    public class BankAccountCancelled : AggregateEvent<BankAccountAggregate>
    {
    }
}