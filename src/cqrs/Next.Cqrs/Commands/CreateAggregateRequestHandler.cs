using System.Threading;
using System.Threading.Tasks;
using Next.Abstractions.Domain;
using Next.Abstractions.Domain.Persistence;
using Next.Abstractions.Domain.Specifications;
using Next.Application.Pipelines;

namespace Next.Cqrs.Commands
{
    public abstract class CreateAggregateRequestHandler<TAggregate, TIdentity, TState, TCommand, TCommandResponse> : CommandHandler<TCommand, TCommandResponse>
        where TCommand : CreateAggregateCommand<TAggregate, TIdentity, TCommandResponse>
        where TAggregate : IAggregateRoot<TIdentity, TState>
        where TIdentity : IIdentity
        where TState: IState
        where TCommandResponse : CommandResponse, new()
    {
        private readonly IAggregateRepository<TAggregate, TIdentity, TState> _aggregateRepository;
        
        protected CreateAggregateRequestHandler(IAggregateRepository<TAggregate, TIdentity, TState> aggregateRepository)
        {
            _aggregateRepository = aggregateRepository;
        }

        protected abstract TCommandResponse Execute(
            TCommand request,
            TAggregate aggregate,
            IOperationContext context,
            CancellationToken cancellationToken = default);

        public override async Task<TCommandResponse> Execute(
            TCommand request,
            IOperationContext context,
            CancellationToken cancellationToken = default)
        {
            var aggregate = await _aggregateRepository.FindOrDefault(request.GetIdentity());

            if (!AggregateSpecs
                .AggregateIsNew
                .IsSatisfiedBy(aggregate))
            {
                return CommandResponse.Fail<TCommandResponse>(DomainErrors.IsNotNew);
            }

            var response = Execute(
                request,
                aggregate,
                context,
                cancellationToken);

            if (response.IsSuccess)
            {
                await _aggregateRepository.Save(aggregate);
                
                // set current aggregate root in operation context
                context.Features.Set(aggregate);
            }

            return response;
        }
    }
}