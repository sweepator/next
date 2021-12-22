namespace Next.Abstractions.Domain.Entities
{
    public interface IEntity
    {
        IIdentity Id { get; }
    }
    
    public interface IEntity<out TIdentity> : IEntity
        where TIdentity : IIdentity
    {
        new TIdentity Id { get; }
        
        IIdentity IEntity.Id => Id;
    }
}