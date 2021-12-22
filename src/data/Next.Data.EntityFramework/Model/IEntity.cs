namespace Next.Data.EntityFramework.Model
{
    public interface IEntity
    {
        object Id { get; }
    }

    public interface IEntity<out T>: IEntity
    {
        new T Id { get; }

        object IEntity.Id => this.Id;
    }
}
