using System.Collections.Generic;

namespace Next.Data.SqlServer
{
    public class SqlServerOptions
    {
        public string ConnectionString { get; set; }
        public double TimeoutSeconds { get; set; } = 30;
    }
}
