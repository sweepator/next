using Microsoft.EntityFrameworkCore;

namespace Next.Data.EntityFramework
{
    public interface IDbContextFactory
    {
        DbContext Create();
    }
    
    public interface IDbContextFactory<out TDbContext> : IDbContextFactory
        where TDbContext : DbContext
    {
        new DbContext Create() => CreateContext();
        
        TDbContext CreateContext();
    }
}
