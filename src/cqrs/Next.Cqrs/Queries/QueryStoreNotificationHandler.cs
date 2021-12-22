using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Next.Abstractions.Domain;
using Next.Application.Contracts;
using Next.Application.Pipelines;

namespace Next.Cqrs.Queries
{
    public sealed class QueryStoreNotificationHandler<TNotification>: NotificationHandler<TNotification>
        where TNotification: INotification<IDomainEvent>
    {
        private readonly IEnumerable<IAsyncQueryStoreUpdater> _queryStoreManagers;

        public QueryStoreNotificationHandler(IEnumerable<IAsyncQueryStoreUpdater> queryStoreManagers)
        {
            _queryStoreManagers = queryStoreManagers;
        }
        
        public override async Task Execute(
            TNotification notification, 
            IOperationContext context, 
            CancellationToken cancellationToken = default)
        {
            var tasks = _queryStoreManagers
                .Select(async queryStoreManager =>
                {
                    await queryStoreManager.Update(
                        new[]
                        {
                            notification.Content
                        },
                        cancellationToken);
                });

            await Task
                .WhenAll(tasks)
                .ConfigureAwait(false);
        }
    }
}