using System.Threading.Tasks;

namespace Next.Data.SqlServer
{
    public interface ISqlDbContextFactory
    {
        ISqlDbContext GetSqlDbContext(string connectionString);
    }
}