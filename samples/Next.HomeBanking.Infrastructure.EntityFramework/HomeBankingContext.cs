using Microsoft.EntityFrameworkCore;
using Next.Cqrs.Queries.EntityFramework;
using Next.Cqrs.Queries.Projections;

namespace Next.HomeBanking.Infrastructure.EntityFramework
{
    public class HomeBankingContext : QueryStoreDbContext
    {
        public HomeBankingContext(
            DbContextOptions options,
            IProjectionModelDefinitionService projectionModelDefinitionService) 
            : base(options, projectionModelDefinitionService)
        {
        }
    }
}