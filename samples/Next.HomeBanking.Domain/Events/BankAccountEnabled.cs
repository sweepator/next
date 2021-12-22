using Next.Abstractions.Domain;
using Next.HomeBanking.Domain.Aggregates;

namespace Next.HomeBanking.Domain.Events
{
    public class BankAccountEnabled : AggregateEvent<BankAccountAggregate>
    {
        public bool Enabled { get; }

        public BankAccountEnabled(bool enabled)
        {
            Enabled = enabled;
        }
    }
}