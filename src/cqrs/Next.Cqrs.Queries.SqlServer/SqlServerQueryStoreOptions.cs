using Next.Cqrs.Queries.Projections;

namespace Next.Cqrs.Queries.SqlServer
{
    public class SqlServerQueryStoreOptions<TProjectionModel>
        where TProjectionModel : class, IProjectionModel
    {
        public string ConnectionString { get; set; }    
    }
}