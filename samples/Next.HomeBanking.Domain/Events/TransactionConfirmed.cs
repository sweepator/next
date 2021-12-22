using System;
using Next.Abstractions.Domain;
using Next.HomeBanking.Domain.Aggregates;

namespace Next.HomeBanking.Domain.Events
{
    public class TransactionConfirmed : AggregateEvent<BankAccountAggregate>
    {
        public Id Id { get; }
        public Id ReferenceTransactionId { get; }
        public BankAccountId Source { get; }
        public BankAccountId Target { get; }
        public DateTime Timestamp { get; }

        public TransactionConfirmed(
            Id id,
            Id referenceTransactionId,
            BankAccountId source,
            BankAccountId target, 
            DateTime timestamp)
        {
            Id = id;
            ReferenceTransactionId = referenceTransactionId;
            Source = source;
            Target = target;
            Timestamp = timestamp;
        }
    }
}