using System.Collections.Generic;

namespace Next.Data.DbUp.SqlServer
{
    public class SqlServerDataMigrationsOptions
    {
        public string ConnectionString { get; set; }
        public double TimeoutSeconds { get; set; } = 30;
        public IDictionary<string, string> Variables { get; set; } = new Dictionary<string, string>();


        public SqlServerDataMigrationsOptions WitConnectionString(string connectionString)
        {
            ConnectionString = connectionString;
            return this;
        }
    }
}