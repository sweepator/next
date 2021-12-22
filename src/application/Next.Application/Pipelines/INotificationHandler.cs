using System.Threading;
using System.Threading.Tasks;
using Next.Application.Contracts;

namespace Next.Application.Pipelines
{
    public interface INotificationHandler<in TNotification> : MediatR.INotificationHandler<TNotification>
        where TNotification : INotification
    {
        Task Execute(
            TNotification notification,
            IOperationContext context,
            CancellationToken cancellationToken = default);
    }
}