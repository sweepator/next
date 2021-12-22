using System;
using Next.Abstractions.Domain;
using Next.Cqrs.Queries.Projections;
using Next.HomeBanking.Domain.Aggregates;
using Next.HomeBanking.Domain.Events;
using Next.HomeBanking.Domain.SharedKernel;

namespace Next.HomeBanking.Application.Queries
{
    public class BankAccountTransactionProjection: 
        IProjectionModel,
        IProjectionModelFor<BankAccountAggregate, BankAccountId, TransactionCreated>,
        IProjectionModelFor<BankAccountAggregate, BankAccountId, TransactionConfirmed>,
        IProjectionModelFor<BankAccountAggregate, BankAccountId, TransactionStarted>,
        IProjectionModelFor<BankAccountAggregate, BankAccountId, TransactionFinished>,
        IProjectionModelFor<BankAccountAggregate, BankAccountId, TransactionCancelled>
    {
        public string Id { get; private set; }
        public string ReferenceId { get; private set; }
        public string AccountId { get; private set; }
        public string TargetAccountId { get; private set; }
        public string SourceAccountId { get; private set; }
        public TransactionType? Type { get; private set; }
        public TransactionState? State { get; private set; }
        public decimal Balance { get; private set; }
        public decimal Amount { get; private set; }
        public int? Version { get; set; }
        public DateTime? CreateDate { get; private set; }
        public DateTime? UpdateDate { get; private set; }
        
        public void Apply(
            IProjectionModelContext context, 
            IDomainEvent<BankAccountAggregate, BankAccountId, TransactionCreated> domainEvent)
        {
            Id = domainEvent.AggregateEvent.Id.Value;
            AccountId = domainEvent.AggregateIdentity.Value;
            Amount = domainEvent.AggregateEvent.Amount;
            Balance = domainEvent.AggregateEvent.Type == TransactionType.Debit
                ? domainEvent.AggregateEvent.BalanceBeforeTransaction - domainEvent.AggregateEvent.Amount
                : domainEvent.AggregateEvent.BalanceBeforeTransaction + domainEvent.AggregateEvent.Amount;
            Version = domainEvent.Version;
            CreateDate = domainEvent.Timestamp;
            Type = domainEvent.AggregateEvent.Type;
            State = domainEvent.AggregateEvent.State;
        }

        public void Apply(
            IProjectionModelContext context, 
            IDomainEvent<BankAccountAggregate, BankAccountId, TransactionConfirmed> domainEvent)
        {
            Id = domainEvent.AggregateEvent.Id.Value;
            AccountId = domainEvent.AggregateIdentity.Value;
            Version = domainEvent.Version;
            UpdateDate = domainEvent.Timestamp;
            State = TransactionState.Confirmed;
        }

        public void Apply(
            IProjectionModelContext context, 
            IDomainEvent<BankAccountAggregate, BankAccountId, TransactionStarted> domainEvent)
        {
            Id = domainEvent.AggregateEvent.Id.Value;
            AccountId = domainEvent.AggregateIdentity.Value;
            TargetAccountId = domainEvent.AggregateEvent.TargetAccountId.Value;
            Balance = domainEvent.AggregateEvent.BalanceBeforeTransaction - domainEvent.AggregateEvent.Amount;
            Amount = domainEvent.AggregateEvent.Amount;
            Version = domainEvent.Version;
            CreateDate = domainEvent.Timestamp;
            Type = TransactionType.Debit;
            State = TransactionState.Pending;
        }

        public void Apply(
            IProjectionModelContext context, 
            IDomainEvent<BankAccountAggregate, BankAccountId, TransactionFinished> domainEvent)
        {
            Id = domainEvent.AggregateEvent.Id.Value;
            AccountId = domainEvent.AggregateIdentity.Value;
            SourceAccountId = domainEvent.AggregateEvent.SourceAccountId.Value;
            ReferenceId = domainEvent.AggregateEvent.ReferenceTransactionId.Value;
            Balance = domainEvent.AggregateEvent.BalanceBeforeTransaction + domainEvent.AggregateEvent.Amount;
            Amount = domainEvent.AggregateEvent.Amount;
            Version = domainEvent.Version;
            CreateDate= domainEvent.Timestamp;
            Type = TransactionType.Credit;
            State = TransactionState.Confirmed;
        }

        public void Apply(
            IProjectionModelContext context, 
            IDomainEvent<BankAccountAggregate, BankAccountId, TransactionCancelled> domainEvent)
        {
            State = TransactionState.Cancelled;
            Balance = Type == TransactionType.Credit ? Balance - Amount : Balance + Amount;
            UpdateDate = domainEvent.Timestamp;
            Version = domainEvent.Version;
        }
    }
}