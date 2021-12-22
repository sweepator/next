using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Next.Abstractions.Domain;
using Next.Abstractions.Domain.Entities;
using Next.HomeBanking.Domain.Aggregates;
using Next.HomeBanking.Domain.SharedKernel;

namespace Next.HomeBanking.Domain.Entities
{
    public class Transaction : Entity<Id>
    {
        public decimal Amount { get; }
        public BankAccountId SourceBankAccountId { get; }
        public BankAccountId TargetBankAccountId { get; }
        public Id ReferenceTransactionId { get; internal set; }
        public TransactionState State { get; internal set; }
        public TransactionType Type { get; }
        public DateTime? StartedAt { get; }
        public DateTime? FinishedAt { get; }
        public DateTime? ConfirmedAt { get; internal set; }
        public DateTime? CancelledAt { get; internal set; }

        [JsonConstructor]
        public Transaction(
            Id id,
            decimal amount,
            BankAccountId sourceBankAccountId,
            BankAccountId targetBankAccountId,
            Id referenceTransactionId,
            TransactionState state,
            TransactionType type,
            DateTime? startedAt,
            DateTime? finishedAt,
            DateTime? confirmedAt,
            DateTime? cancelledAt)
            : base(id) =>
            (Id,
                Amount,
                SourceBankAccountId,
                TargetBankAccountId,
                ReferenceTransactionId,
                State,
                Type,
                StartedAt,
                FinishedAt,
                ConfirmedAt,
                CancelledAt) =
            (id,
                amount,
                sourceBankAccountId,
                targetBankAccountId,
                referenceTransactionId,
                state,
                type,
                startedAt,
                finishedAt,
                confirmedAt,
                cancelledAt);
        
        public Transaction(
            Id id, 
            decimal amount, 
            TransactionState state, 
            TransactionType type,
            DateTime? startedAt = null,
            DateTime? finishedAt = null,
            BankAccountId targetBankAccountId = null, 
            BankAccountId sourceBankAccountId = null,
            Id referenceTransactionId = null)
            : base(id)
        {
            Amount = amount;
            State = state;
            Type = type;
            StartedAt = startedAt;
            FinishedAt = finishedAt;
            TargetBankAccountId = targetBankAccountId;
            SourceBankAccountId = sourceBankAccountId;
            ReferenceTransactionId = referenceTransactionId;
        }
    }
}