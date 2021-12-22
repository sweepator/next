using Next.Abstractions.Domain;

namespace Next.Cqrs.Commands
{
    public abstract class AggregateCommand<TAggregate, TIdentity, TCommandResponse>: ICommand<TCommandResponse>, IAggregateCommand
        where TCommandResponse : ICommandResponse
        where TAggregate: IAggregateRoot<TIdentity>
        where TIdentity: IIdentity
    {
        public TIdentity Id { get; set; }

        IIdentity IAggregateCommand.Id => Id;
    }
}