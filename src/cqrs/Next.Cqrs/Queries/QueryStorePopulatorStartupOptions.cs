namespace Next.Cqrs.Queries
{
    public class QueryStorePopulatorStartupOptions<TProjectionModel>
    {
        public bool Enabled { get; set; }
        public bool Rebuild { get; set; }
    }
}