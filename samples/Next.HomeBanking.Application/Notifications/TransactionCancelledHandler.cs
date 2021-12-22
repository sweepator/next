using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Next.Abstractions.Domain;
using Next.Abstractions.Domain.Persistence;
using Next.Application.Contracts;
using Next.Application.Pipelines;
using Next.HomeBanking.Domain.Aggregates;
using Next.HomeBanking.Domain.Events;

namespace Next.HomeBanking.Application.Notifications
{
    public class TransactionCancelledHandler : NotificationHandler<Notification<DomainEvent<BankAccountAggregate, BankAccountId, TransactionCancelled>>>
    {
        private readonly IAggregateRepository<BankAccountAggregate, BankAccountId, BankAccountState> _aggregateRepository;
        private readonly ILogger<TransactionCancelledHandler> _logger;

        public TransactionCancelledHandler(
            IAggregateRepository<BankAccountAggregate, BankAccountId, BankAccountState> aggregateRepository,
            ILogger<TransactionCancelledHandler> logger)
        {
            _aggregateRepository = aggregateRepository;
            _logger = logger;
        }

        public override async Task Execute(
            Notification<DomainEvent<BankAccountAggregate, BankAccountId, TransactionCancelled>> notification,
            IOperationContext context,
            CancellationToken cancellationToken = default)
        {
            if (notification.Content.AggregateEvent.ReferenceTransactionId == null)
            {
                return;
            }
            
            var accountId = notification.Content.AggregateEvent.SourceBankAccountId ??
                            notification.Content.AggregateEvent.TargetBankAccountId;
            var transactionId = notification.Content.AggregateEvent.ReferenceTransactionId;

            _logger.Info(
                "Cancelling transaction {TransactionId} in account {AccountId}",
                transactionId,
                accountId);

            // cancel referenced transaction in the account
            var bankAccount = await _aggregateRepository.Find(accountId);
            var result = bankAccount.CancelTransaction(transactionId);

            _logger.Info(
                "Cancelled transaction {TransactionId} in account {AccountId} result: {Result}",
                transactionId,
                accountId,
                result);

            if (!result.HasErrors)
            {
                await _aggregateRepository.Save(bankAccount);
            }
        }
    }
}