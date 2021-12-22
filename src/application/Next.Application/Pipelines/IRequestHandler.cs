using Next.Application.Contracts;
using System.Threading;
using System.Threading.Tasks;

namespace Next.Application.Pipelines
{
    public interface IRequestHandler<in TRequest, TResponse> : MediatR.IRequestHandler<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
        where TResponse : IResponse
    {
        Task<TResponse> Execute(
            TRequest request,
            IOperationContext context,
            CancellationToken cancellationToken = default);
    }
    
    
    public interface IRequestHandler<in TRequest> : MediatR.IRequestHandler<TRequest>
        where TRequest : IRequest
    {
        Task Execute(
            TRequest request,
            IOperationContext context,
            CancellationToken cancellationToken = default);
    }
}
