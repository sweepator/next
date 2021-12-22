using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Next.Abstractions.Domain;
using Next.Application.Contracts;
using Next.Application.Pipelines;
using Next.Cqrs.Queries;
using Next.HomeBanking.Application.Queries;
using Next.HomeBanking.Domain.Aggregates;
using Next.HomeBanking.Domain.Events;

namespace Next.HomeBanking.Application.Notifications
{
    public class BankAccountCancelledHandler : NotificationHandler<Notification<DomainEvent<BankAccountAggregate, BankAccountId, BankAccountCancelled>>>
    {
        private readonly IQueryableQueryStore<BankAccountTransactionProjection> _queryStore;
        private readonly ILogger<BankAccountCancelledHandler> _logger;

        public BankAccountCancelledHandler(
            IQueryableQueryStore<BankAccountTransactionProjection> queryStore,
            ILogger<BankAccountCancelledHandler> logger)
        {
            _queryStore = queryStore;
            _logger = logger;
        }
        
        public override async Task Execute(
            Notification<DomainEvent<BankAccountAggregate, BankAccountId, BankAccountCancelled>> notification, 
            IOperationContext context, 
            CancellationToken cancellationToken = default)
        {
            var cancelledAccountId = notification.Content.AggregateIdentity.Value;
            
            _logger.Info(
                "Removing all transactions related with bank account {BankAccountId}",
                cancelledAccountId);

            await _queryStore.Delete(
                o => o.AccountId.Equals(cancelledAccountId),
                cancellationToken);
            
            _logger.Info(
                "Removed all transactions related with bank account {BankAccountId}",
                notification.Content.AggregateIdentity.Value);
        }
    }
}