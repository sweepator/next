using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json.Serialization;
using Next.Abstractions.Domain;
using Next.HomeBanking.Domain.Entities;
using Next.HomeBanking.Domain.SharedKernel;
using Next.HomeBanking.Domain.Events;

namespace Next.HomeBanking.Domain.Aggregates
{
    public class BankAccountState : State
    {
        private readonly List<Transaction> _transactions = new();
        
        public string Owner { get; private set; }
        
        public string Iban { get; private set; }
        
        public decimal Balance { get; private set; }
        
        public bool Enabled { get; private set; }
        
        public bool IsCancelled { get; private set; }

        public Transaction[] Transactions
        {
            get => _transactions.ToArray();
            init => _transactions = new List<Transaction>(value);
        }
        
        public BankAccountState()
        {
        }

        [JsonConstructor]
        public BankAccountState(
            int version,
            string owner,
            string iban,
            decimal balance,
            bool enabled,
            bool isCancelled,
            Transaction[] transactions) : base(version) =>
            (Owner, Iban, Balance, Enabled, IsCancelled, Transactions) =
            (owner, iban, balance, enabled, isCancelled, transactions);
        
        private void On(BankAccountCreated ev)
        {
            Owner = ev.Owner;
            Enabled = ev.Enabled;
            Iban = ev.Iban;
            Balance = 0;
        }
        
        private void On(BankAccountEnabled ev)
        {
            Enabled = ev.Enabled;
        }
        
        private void On(TransactionCreated ev)
        {
            if (ev.Type == TransactionType.Credit)
            {
                Balance += ev.Amount;
            }
            else
            {
                Balance -= ev.Amount;
            }
            
            _transactions.Add(new Transaction(
                ev.Id,
                ev.Amount,
                ev.State,
                ev.Type,
                ev.CreatedDate
                ));
        }
        
        private void On(TransactionConfirmed ev)
        {
            var transaction = Transactions.Single(o => o.Id.Equals(ev.Id));
            transaction.State = TransactionState.Confirmed;
            transaction.ConfirmedAt = ev.Timestamp;
            transaction.ReferenceTransactionId = ev.ReferenceTransactionId;
        }
        
        private void On(TransactionStarted ev)
        {
            Balance -= ev.Amount;
            _transactions.Add(new Transaction(
                ev.Id,
                ev.Amount,
                TransactionState.Pending,
                TransactionType.Debit,
                startedAt:ev.Timestamp,
                targetBankAccountId: ev.TargetAccountId));
        }
        
        private void On(TransactionFinished ev)
        {
            Balance += ev.Amount;
            _transactions.Add(new Transaction(
                ev.Id,
                ev.Amount,
                TransactionState.Confirmed,
                TransactionType.Credit,
                finishedAt:ev.Timestamp,
                sourceBankAccountId: ev.SourceAccountId,
                referenceTransactionId: ev.ReferenceTransactionId));
        }
        
        private void On(BankAccountCancelled ev)
        {
            IsCancelled = true;
        }
        
        private void On(TransactionCancelled ev)
        {
            var transaction = _transactions.Single(o => o.Id.Equals(ev.Id));
            transaction.State = TransactionState.Cancelled;
            transaction.CancelledAt = ev.Timestamp;
            Balance = transaction.Type == TransactionType.Credit
                ? Balance - transaction.Amount
                : Balance + transaction.Amount;
        }
    }
} 