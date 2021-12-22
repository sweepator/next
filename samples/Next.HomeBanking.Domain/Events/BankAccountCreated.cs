using Next.Abstractions.Domain;
using Next.HomeBanking.Domain.Aggregates;

namespace Next.HomeBanking.Domain.Events
{
    public class BankAccountCreated: AggregateEvent<BankAccountAggregate>
    {
        public string Owner { get; }
        public string Iban { get; }
        public bool Enabled { get; }
        
        public BankAccountCreated(
            string owner,
            string iban,
            bool enabled)
        {
            Owner = owner;
            Iban = iban;
            Enabled = enabled;
        }
    }
}