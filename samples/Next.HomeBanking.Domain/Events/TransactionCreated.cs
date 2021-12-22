using System;
using Next.Abstractions.Domain;
using Next.HomeBanking.Domain.Aggregates;
using Next.HomeBanking.Domain.SharedKernel;

namespace Next.HomeBanking.Domain.Events
{
    public class TransactionCreated : AggregateEvent<BankAccountAggregate>
    {
        public Id Id { get; }
        public decimal BalanceBeforeTransaction { get; }
        public decimal BalanceResult { get; }
        public decimal Amount { get; }
        public TransactionState State { get; }
        public TransactionType Type { get; }
        public DateTime CreatedDate { get; }

        public TransactionCreated(Id id,
            decimal balanceBeforeTransaction,
            decimal balanceResult,
            decimal amount,
            TransactionType type,
            TransactionState state,
            DateTime createdDate)
        {
            Amount = amount;
            Type = type;
            State = state;
            CreatedDate = createdDate;
            Id = id;
            BalanceBeforeTransaction = balanceBeforeTransaction;
            BalanceResult = balanceResult;
        }
    }
}