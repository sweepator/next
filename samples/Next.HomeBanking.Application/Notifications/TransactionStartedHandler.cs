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
    public class TransactionStartedHandler : NotificationHandler<Notification<DomainEvent<BankAccountAggregate, BankAccountId, TransactionStarted>>>
    {
        private readonly IAggregateRepository<BankAccountAggregate, BankAccountId, BankAccountState> _aggregateRepository;
        private readonly ILogger<TransactionFinishHandler> _logger;

        public TransactionStartedHandler(
            IAggregateRepository<BankAccountAggregate, BankAccountId, BankAccountState> aggregateRepository,
            ILogger<TransactionFinishHandler> logger)
        {
            _aggregateRepository = aggregateRepository;
            _logger = logger;
        }
        
        public override async Task Execute(
            Notification<DomainEvent<BankAccountAggregate, BankAccountId, TransactionStarted>> notification, 
            IOperationContext context, 
            CancellationToken cancellationToken = default)
        {
            _logger.Info(
                "Finishing transaction in the source account: {AccountId} {TargetAccountId}",
                notification.Content.AggregateIdentity.Value,
                notification.Content.AggregateEvent.TargetAccountId.Value);
            
            var bankAccount = await _aggregateRepository.Find(notification.Content.AggregateEvent.TargetAccountId);
            
            var result = bankAccount.FinishTransaction(
                notification.Content.AggregateIdentity,
                notification.Content.AggregateEvent.Id,
                notification.Content.AggregateEvent.Amount);
            
            _logger.Info(
                "Finish transaction result: {AccountId} {SourceAccountId}",
                notification.Content.AggregateIdentity.Value,
                notification.Content.AggregateEvent.TargetAccountId.Value,
                result);

            if (!result.HasErrors)
            {
                await _aggregateRepository.Save(bankAccount);
            }
        }
    }
}