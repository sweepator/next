using System;
using Next.Cqrs.Queries.SqlServer;

namespace Next.Cqrs.Configuration
{
    public static class ProjectionsBuilderExtensions
    {
        public static ISqlServerProjectionBuilder UseSqlServer(
            this IProjectionsBuilder projectionsBuilder,
            string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString));
            }
            
            return new SqlServerProjectionBuilder(
                projectionsBuilder,
                connectionString);
        }
    }
}