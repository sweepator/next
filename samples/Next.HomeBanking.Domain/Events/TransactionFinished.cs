using System;
using Next.Abstractions.Domain;
using Next.HomeBanking.Domain.Aggregates;

namespace Next.HomeBanking.Domain.Events
{
    public class TransactionFinished: AggregateEvent<BankAccountAggregate>
    {
        public Id Id { get; }
        public Id ReferenceTransactionId { get; }
        public decimal BalanceResult { get; }
        public decimal BalanceBeforeTransaction { get; }
        public decimal Amount { get; }
        public BankAccountId SourceAccountId { get; }
        public DateTime Timestamp { get; }       
        
        public TransactionFinished(Id id,
            decimal balanceBeforeTransaction,
            decimal amount,
            BankAccountId sourceAccountId,
            Id referenceTransactionId,
            decimal balanceResult,
            DateTime timestamp)
        {
            Amount = amount;
            SourceAccountId = sourceAccountId;
            ReferenceTransactionId = referenceTransactionId;
            BalanceResult = balanceResult;
            Timestamp = timestamp;
            Id = id;
            BalanceBeforeTransaction = balanceBeforeTransaction;
        }
    }
}