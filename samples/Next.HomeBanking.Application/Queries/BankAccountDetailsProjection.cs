using System;
using Next.Abstractions.Domain;
using Next.Cqrs.Queries.Projections;
using Next.HomeBanking.Domain.Aggregates;
using Next.HomeBanking.Domain.Events;

namespace Next.HomeBanking.Application.Queries
{
    public class BankAccountDetailsProjection : IProjectionModel,
        IProjectionModelFor<BankAccountAggregate, BankAccountId, BankAccountCreated>,
        IProjectionModelFor<BankAccountAggregate, BankAccountId, BankAccountEnabled>,
        IProjectionModelFor<BankAccountAggregate, BankAccountId, TransactionCreated>,
        IProjectionModelFor<BankAccountAggregate, BankAccountId, TransactionStarted>,
        IProjectionModelFor<BankAccountAggregate, BankAccountId, TransactionFinished>,
        IProjectionModelFor<BankAccountAggregate, BankAccountId, BankAccountCancelled>
    {
        public string Id { get; set; }
        public string Owner { get; set; }
        public string Iban { get; private set; }
        public bool Enabled { get; private set; }
        public int Transactions { get; private set; }
        public decimal Balance { get; private set; }
        public DateTime? CreateDate { get; private set; }
        public DateTime? UpdateDate { get; private set; }
        public int? Version { get; set; }
        
        public void Apply(
            IProjectionModelContext context, 
            IDomainEvent<BankAccountAggregate, BankAccountId, BankAccountCreated> domainEvent)
        {
            Id = domainEvent.AggregateIdentity.Value;
            Owner = domainEvent.AggregateEvent.Owner;
            Iban = domainEvent.AggregateEvent.Iban;
            Enabled = domainEvent.AggregateEvent.Enabled;
            CreateDate = domainEvent.Timestamp;
        }

        public void Apply(
            IProjectionModelContext context, 
            IDomainEvent<BankAccountAggregate, BankAccountId, BankAccountEnabled> domainEvent)
        {
            Id = domainEvent.AggregateIdentity.Value;
            Enabled = domainEvent.AggregateEvent.Enabled;
            UpdateDate = domainEvent.Timestamp;
        }

        public void Apply(
            IProjectionModelContext context, 
            IDomainEvent<BankAccountAggregate, BankAccountId, TransactionCreated> domainEvent)
        {
            Id = domainEvent.AggregateIdentity.Value;
            Balance = domainEvent.AggregateEvent.BalanceResult;
            UpdateDate = domainEvent.Timestamp;
            Transactions++;
        }

        public void Apply(
            IProjectionModelContext context, 
            IDomainEvent<BankAccountAggregate, BankAccountId, TransactionStarted> domainEvent)
        {
            Id = domainEvent.AggregateIdentity.Value;
            Balance = domainEvent.AggregateEvent.BalanceResult;
            UpdateDate = domainEvent.Timestamp;
            Transactions++;
        }

        public void Apply(
            IProjectionModelContext context, 
            IDomainEvent<BankAccountAggregate, BankAccountId, TransactionFinished> domainEvent)
        {
            Id = domainEvent.AggregateIdentity.Value;
            Balance = domainEvent.AggregateEvent.BalanceResult;
            UpdateDate = domainEvent.Timestamp;
            Transactions++;
        }

        public void Apply(
            IProjectionModelContext context, 
            IDomainEvent<BankAccountAggregate, BankAccountId, BankAccountCancelled> domainEvent)
        {
            context.MarkForDeletion();
        }
    }
}