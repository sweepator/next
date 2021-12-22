using System;
using Next.Application.Contracts;
using System.Threading;
using System.Threading.Tasks;

namespace Next.Application.Pipelines
{
    public abstract class RequestHandler<TRequest, TResponse> :  IRequestHandler<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
        where TResponse : IResponse
    {
        public abstract Task<TResponse> Execute(
            TRequest request, 
            IOperationContext context, 
            CancellationToken cancellationToken = default);

        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken)
        {
            var operationContext = OperationContextAccessor.Instance.Context;

            if (operationContext == null)
            {
                throw new InvalidOperationException("Operation context is null.");
            }
            
            return await Execute(
                request, 
                operationContext,
                cancellationToken);
        }
    }
}
