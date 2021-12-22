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
    public class TransactionFinishHandler : NotificationHandler<Notification<DomainEvent<BankAccountAggregate, BankAccountId, TransactionFinished>>>
    {
        private readonly IAggregateRepository<BankAccountAggregate, BankAccountId, BankAccountState> _aggregateRepository;
        private readonly ILogger<TransactionFinishHandler> _logger;

        public TransactionFinishHandler(
            IAggregateRepository<BankAccountAggregate, BankAccountId, BankAccountState> aggregateRepository,
            ILogger<TransactionFinishHandler> logger)
        {
            _aggregateRepository = aggregateRepository;
            _logger = logger;
        }
        
        public override async Task Execute(
            Notification<DomainEvent<BankAccountAggregate, BankAccountId, TransactionFinished>> notification, 
            IOperationContext context, 
            CancellationToken cancellationToken = default)
        {
            _logger.Info(
                "Confirming transaction in the source account: {AccountId} {SourceAccountId}",
                notification.Content.AggregateIdentity.Value,
                notification.Content.AggregateEvent.SourceAccountId.Value);
            
            // confirm the transaction that in the source account
            var bankAccount = await _aggregateRepository.Find(notification.Content.AggregateEvent.SourceAccountId);
            
            var result = bankAccount.ConfirmTransaction(
                notification.Content.AggregateEvent.ReferenceTransactionId,
                notification.Content.AggregateEvent.Id);
            
            _logger.Info(
                "Confirm transaction result: {AccountId} {SourceAccountId}",
                notification.Content.AggregateIdentity.Value,
                notification.Content.AggregateEvent.SourceAccountId.Value,
                result);

            if (!result.HasErrors)
            {
                await _aggregateRepository.Save(bankAccount);
            }
        }
    }
}