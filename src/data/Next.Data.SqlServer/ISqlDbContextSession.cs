using System.Threading.Tasks;

namespace Next.Data.SqlServer
{
    public interface ISqlDbContextSession
    {
        ISqlDbContext GetSqlDbContext(string connectionString);
    }
}