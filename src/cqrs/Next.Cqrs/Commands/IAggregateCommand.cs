using Next.Abstractions.Domain;

namespace Next.Cqrs.Commands
{
    public interface IAggregateCommand
    {
        IIdentity Id { get; }
    }
}