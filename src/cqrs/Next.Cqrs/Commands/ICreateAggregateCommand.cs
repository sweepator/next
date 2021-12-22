using Next.Abstractions.Domain;

namespace Next.Cqrs.Commands
{
    public interface ICreateAggregateCommand<out TIdentity>
        where TIdentity: IIdentity
    {
        TIdentity GetIdentity();
    }
}