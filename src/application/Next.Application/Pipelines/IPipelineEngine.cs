using Next.Application.Contracts;
using System.Threading;
using System.Threading.Tasks;

namespace Next.Application.Pipelines
{
    public interface IPipelineEngine
    {
        Task<TResponse> Execute<TResponse>(
            IRequest<TResponse> request,
            CancellationToken cancellationToken = default)
            where TResponse : IResponse;

        Task Publish<TNotification>(
            TNotification notification,
            CancellationToken cancellationToken = default)
            where TNotification : INotification;
    }
}
