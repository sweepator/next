using Next.Application.Contracts;
using System.Threading;
using System.Threading.Tasks;

namespace Next.Application.Pipelines
{
    internal sealed class PipelineEngine : IPipelineEngine
    {
        private readonly MediatR.IMediator _mediator;
        private readonly OperationContextAccessor _operationContextAccessor;

        public PipelineEngine(
            MediatR.IMediator mediator,
            OperationContextAccessor operationContextAccessor)
        {
            _mediator = mediator;
            _operationContextAccessor = operationContextAccessor;
        }

        public async Task<TResponse> Execute<TResponse>(
            IRequest<TResponse> request, 
            CancellationToken cancellationToken = default) 
            where TResponse : IResponse
        {
            _operationContextAccessor.Context ??= new OperationContext();
            return await _mediator.Send(request, cancellationToken);
        }
        
        public async Task Publish<TNotification>(
            TNotification notification, 
            CancellationToken cancellationToken = default)
            where TNotification: INotification
        {
            _operationContextAccessor.Context ??= new OperationContext();
            await _mediator.Publish(
                notification, 
                cancellationToken);
        }
    }
}
