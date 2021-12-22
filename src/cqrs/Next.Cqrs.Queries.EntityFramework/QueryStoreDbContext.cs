using Microsoft.EntityFrameworkCore;
using Next.Cqrs.Queries.Projections;
using Next.Data.EntityFramework;

namespace Next.Cqrs.Queries.EntityFramework
{
    public class QueryStoreDbContext : BaseDbContext
    {
        private readonly IProjectionModelDefinitionService _projectionModelDefinitionService;
        
        public QueryStoreDbContext(
            DbContextOptions options,
            IProjectionModelDefinitionService projectionModelDefinitionService) 
            : base(options)
        {
            _projectionModelDefinitionService = projectionModelDefinitionService;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.AddProjectionModels(_projectionModelDefinitionService);
        }
    }
}