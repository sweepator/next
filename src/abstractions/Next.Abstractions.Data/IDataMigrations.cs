using System.Threading.Tasks;

namespace Next.Abstractions.Data
{
    public interface IDataMigrations
    {
        void Migrate();
        Task MigrateAsync();
        Task<DataMigration> GetLastMigrationAsync();
    }
}
