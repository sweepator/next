namespace Next.Data.EntityFramework.Model
{
    public interface ISoftDeletableEntity
    {
        public bool IsDeleted { get; set; }
    }
}
