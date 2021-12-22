using System;
using System.Threading;
using System.Threading.Tasks;
using Next.Application.Contracts;

namespace Next.Application.Pipelines
{
    public abstract class NotificationHandler<TNotification> :  INotificationHandler<TNotification>
        where TNotification : INotification
    {
        public async Task Handle(
            TNotification notification, 
            CancellationToken cancellationToken)
        {
            var operationContext = OperationContextAccessor.Instance.Context;

            if (operationContext == null)
            {
                throw new InvalidOperationException("Operation context is null.");
            }
            
            await Execute(
                notification, 
                operationContext, 
                cancellationToken);
        }

        public abstract Task Execute(
            TNotification notification,
            IOperationContext context,
            CancellationToken cancellationToken = default);
    }
}