using Next.Abstractions.Domain;

namespace Next.Cqrs.Commands
{
    public abstract class CreateAggregateCommand<TAggregate, TIdentity, TCommandResponse>: ICreateAggregateCommand<TIdentity>, ICommand<TCommandResponse>
        where TCommandResponse : ICommandResponse
        where TAggregate: IAggregateRoot<TIdentity>
        where TIdentity: IIdentity
    {
        public abstract TIdentity GetIdentity();
    }
}