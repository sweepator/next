using System;
using System.Linq;
using Next.Abstractions.Domain;
using Next.HomeBanking.Domain.Events;
using Next.Core;
using Next.Core.Errors;
using Next.HomeBanking.Domain.SharedKernel;

namespace Next.HomeBanking.Domain.Aggregates
{
    public class BankAccountAggregate : AggregateRoot<BankAccountAggregate, BankAccountId, BankAccountState>
    {
        public static readonly Error BankAccountDisabled = new (nameof(BankAccountDisabled));
        public static readonly Error BankAccountAlreadyCancelled = new (nameof(BankAccountAlreadyCancelled));
        public static readonly Error InvalidAccountOwner = new (nameof(InvalidAccountOwner));
        public static readonly Error InvalidIban = new (nameof(InvalidIban));
        public static readonly Error NotEnoughBalance = new (nameof(NotEnoughBalance));
        public static readonly Error BankAccountAlreadyEnabled = new (nameof(BankAccountAlreadyEnabled));
        public static readonly Error BankAccountAlreadyDisabled = new (nameof(BankAccountAlreadyDisabled));
        public static readonly Error IbanAlreadyExists = new (nameof(IbanAlreadyExists));
        public static readonly Error InvalidTransactionOperation = new (nameof(InvalidTransactionOperation));
        public static readonly Error TransactionNotFound= new (nameof(TransactionNotFound));
        public static readonly Error TransactionCannotBeCancelled= new (nameof(TransactionCannotBeCancelled));
        public static readonly Error TransactionAlreadyCancelled= new (nameof(TransactionAlreadyCancelled));

        private const int TransactionCancelTimeWindowInSeconds = 300;
        
        public BankAccountAggregate(
            BankAccountId id, 
            BankAccountState state)     
            : base(id, state)
        {
        }
        
        public Notification Create(
            string owner,
            string iban,
            bool enabled)
        {
            if (string.IsNullOrEmpty(owner))
            {
                return Notification.Create(InvalidAccountOwner);
            }
            
            if (string.IsNullOrEmpty(iban))
            {
                return Notification.Create(InvalidIban);
            }
            
            On(new BankAccountCreated(
                owner,
                iban,
                enabled));
            
            return Notification.Sucess;
        }
        
        public Notification Enable(bool enable)
        {
            if (State.IsCancelled)
            {
                return Notification.Create(DomainErrors.NotFound);
            }
            
            switch (State.Enabled)
            {
                case true when enable:
                {
                    return Notification.Create(BankAccountAlreadyEnabled);
                }
                case false when !enable:
                {
                    return Notification.Create(BankAccountAlreadyDisabled);
                }
                default:
                {
                    On(new BankAccountEnabled(enable));
                    return Notification.Sucess;
                }
            }
        }

        public bool CanEnable()
        {
            return !State.IsCancelled;
        }
        
        public Notification Deposit(decimal amount)
        {
            if (!State.Enabled)
            {
                return Notification.Create(BankAccountDisabled);
            }
            
            if (State.IsCancelled)
            {
                return Notification.Create(DomainErrors.NotFound);
            }
            
            var balanceResult = State.Balance + amount;
            
            On(new TransactionCreated(
                Next.Abstractions.Domain.Id.New,
                State.Balance,
                balanceResult,
                amount,
                TransactionType.Credit,
                TransactionState.Confirmed,
                DateTime.UtcNow));
            
            return Notification.Sucess;
        }
        
        public bool CanDeposit()
        {
            return !State.IsCancelled && State.Enabled;
        }
        
        public Notification Debit(decimal amount)
        {
            if (!State.Enabled)
            {
                return Notification.Create(BankAccountDisabled);
            }
            
            if (State.IsCancelled)
            {
                return Notification.Create(DomainErrors.NotFound);
            }
            
            if ((State.Balance - amount) < 0)
            {
                return Notification.Create(NotEnoughBalance);
            }

            var balanceResult = State.Balance - amount;
 
            On(new TransactionCreated(
                Next.Abstractions.Domain.Id.New,
                State.Balance,
                balanceResult,
                amount,
                TransactionType.Debit,
                TransactionState.Confirmed,
                DateTime.UtcNow));
            
            return Notification.Sucess;
        }
        
        public bool CanDebit()
        {
            return !State.IsCancelled && State.Enabled && State.Balance > 0;
        }
        
        public Notification StartTransaction(
            BankAccountId targetAccountId,
            decimal amount)
        {
            if (!State.Enabled)
            {
                return Notification.Create(BankAccountDisabled);
            }
            
            if (State.IsCancelled)
            {
                return Notification.Create(DomainErrors.NotFound);
            }
            
            if ((State.Balance - amount) < 0)
            {
                return Notification.Create(NotEnoughBalance);
            }
            
            var balanceResult = State.Balance - amount;
            On(new TransactionStarted(
                Next.Abstractions.Domain.Id.New,
                State.Balance,
                amount,
                targetAccountId,
                balanceResult,
                DateTime.UtcNow));
            
            return Notification.Sucess;
        }
        
        public bool CanTransfer()
        {
            return !State.IsCancelled && State.Enabled && State.Balance > 0;
        }
        
        public Notification FinishTransaction(
            BankAccountId sourceAccountId,
            Id transactionId,
            decimal amount)
        {
            if (!State.Enabled)
            {
                return Notification.Create(BankAccountDisabled);
            }
            
            if (State.IsCancelled)
            {
                return Notification.Create(DomainErrors.NotFound);
            }
            
            var balanceResult = State.Balance + amount;
            
            On(new TransactionFinished(
                Next.Abstractions.Domain.Id.New,
                State.Balance,
                amount,
                sourceAccountId,
                transactionId,
                balanceResult,
                DateTime.UtcNow));
            
            return Notification.Sucess;
        }
        
        public Notification ConfirmTransaction(
            Id transactionId,
            Id referenceTransactionId)
        {
            if (!State.Enabled)
            {
                return Notification.Create(BankAccountDisabled);
            }
            
            if (State.IsCancelled)
            {
                return Notification.Create(DomainErrors.NotFound);
            }

            var transaction = State.Transactions.FirstOrDefault(o => o.Id.Equals(transactionId));
            if (transaction == null)
            {
                return Notification.Create(TransactionNotFound);
            }
            
            if (transaction.State != TransactionState.Pending)
            {
                return Notification.Create(InvalidTransactionOperation);
            }

            On(new TransactionConfirmed(
                transactionId,
                referenceTransactionId,
                Id,
                transaction.SourceBankAccountId,
                DateTime.UtcNow));
            
            return Notification.Sucess;
        }
        
        public Notification Cancel()
        {
            if (State.IsCancelled)
            {
                return Notification.Create(BankAccountAlreadyCancelled);
            }
            
            On(new BankAccountCancelled());
            
            return Notification.Sucess;
        }
        
        public bool CanCancel()
        {
            return !State.IsCancelled;
        }
        
        public Notification CancelTransaction(Id transactionId)
        {
            if (!State.Enabled)
            {
                return Notification.Create(BankAccountDisabled);
            }
            
            if (State.IsCancelled)
            {
                return Notification.Create(BankAccountAlreadyCancelled);
            }
            
            var transaction = State.Transactions.FirstOrDefault(o => o.Id.Equals(transactionId));
            if (transaction == null)
            {
                return Notification.Create(TransactionNotFound);
            }

            if (transaction.State == TransactionState.Cancelled)
            {
                return Notification.Create(TransactionAlreadyCancelled);
            }
            
            if (transaction.State == TransactionState.Confirmed
                && DateTime.UtcNow.Subtract(transaction.ConfirmedAt.GetValueOrDefault()).Seconds > TransactionCancelTimeWindowInSeconds)
            {
                return Notification.Create(TransactionCannotBeCancelled);
            }

            var balanceResult = transaction.Type == TransactionType.Credit
                ? State.Balance - transaction.Amount
                : State.Balance + transaction.Amount;
            
            On(new TransactionCancelled(
                transaction.Id,
                transaction.Type,
                transaction.Amount,
                transaction.SourceBankAccountId,
                transaction.TargetBankAccountId,
                transaction.ReferenceTransactionId,
                balanceResult,
                DateTime.UtcNow));
            
            return Notification.Sucess;
        }
    }
}