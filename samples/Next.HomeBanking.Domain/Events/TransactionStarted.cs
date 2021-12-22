using System;
using Next.Abstractions.Domain;
using Next.HomeBanking.Domain.Aggregates;

namespace Next.HomeBanking.Domain.Events
{
    public class TransactionStarted: AggregateEvent<BankAccountAggregate>
    {
        public Id Id { get; }
        public decimal BalanceBeforeTransaction { get; }
        public decimal Amount { get; }
        public BankAccountId TargetAccountId { get; }
        public decimal BalanceResult { get; }
        public DateTime Timestamp { get; }
        
        public TransactionStarted(Id id,
            decimal balanceBeforeTransaction,
            decimal amount,
            BankAccountId targetAccountId,
            decimal balanceResult,
            DateTime timestamp)
        {
            Amount = amount;
            TargetAccountId = targetAccountId;
            BalanceResult = balanceResult;
            Timestamp = timestamp;
            Id = id;
            BalanceBeforeTransaction = balanceBeforeTransaction;
        }
    }
}