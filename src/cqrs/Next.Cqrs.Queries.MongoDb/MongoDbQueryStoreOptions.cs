using Next.Cqrs.Queries.Projections;

namespace Next.Cqrs.Queries.MongoDb
{
    public class MongoDbQueryStoreOptions<TProjectionModel>
        where TProjectionModel : class, IProjectionModel
    {
        public string ConnectionString { get; set; }    
        public string DatatBase { get; set; }  
    }
}