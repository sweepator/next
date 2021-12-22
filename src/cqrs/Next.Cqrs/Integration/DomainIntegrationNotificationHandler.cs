using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Next.Abstractions.Domain;
using Next.Application.Contracts;
using Next.Application.Pipelines;

namespace Next.Cqrs.Integration
{
    internal sealed class DomainIntegrationNotificationHandler<TNotification>: NotificationHandler<TNotification>
        where TNotification: INotification<IDomainEvent>
    {
        private readonly IEnumerable<IDomainIntegration> _domainIntegrations;
        private readonly ILogger<DomainIntegrationNotificationHandler<TNotification>> _logger;

        public DomainIntegrationNotificationHandler(
            IEnumerable<IDomainIntegration> domainIntegrations,
            ILogger<DomainIntegrationNotificationHandler<TNotification>> logger)
        {
            _domainIntegrations = domainIntegrations;
            _logger = logger;
        }
        
        public override async Task Execute(
            TNotification notification, 
            IOperationContext context, 
            CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Integration notification handler: {Notification}", notification);

            foreach (var domainIntegration in _domainIntegrations)
            {
                await domainIntegration.Publish(notification.Content);
            }

            _logger.LogDebug("Integration notification published: {Notification}", notification);
        }
    }
}