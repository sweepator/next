using System;
using Next.Abstractions.Domain;
using Next.HomeBanking.Domain.Aggregates;
using Next.HomeBanking.Domain.SharedKernel;

namespace Next.HomeBanking.Domain.Events
{
    public class TransactionCancelled : AggregateEvent<BankAccountAggregate>
    {
        public Id Id { get; }
        public TransactionType Type { get; }
        public decimal Amount { get; }
        public BankAccountId SourceBankAccountId { get; }
        public BankAccountId TargetBankAccountId { get; }
        public Id ReferenceTransactionId { get; }
        public DateTime Timestamp { get; }
        public decimal BalanceResult { get; }

        public TransactionCancelled(
            Id id,
            TransactionType type,
            decimal amount,
            BankAccountId sourceBankAccountId,
            BankAccountId targetBankAccountId, 
            Id referenceTransactionId,
            decimal balanceResult,
            DateTime timestamp)
        {
            Id = id;
            Type = type;
            Amount = amount;
            SourceBankAccountId = sourceBankAccountId;
            TargetBankAccountId = targetBankAccountId;
            ReferenceTransactionId = referenceTransactionId;
            Timestamp = timestamp;
            BalanceResult = balanceResult;
        }
    }
}