using System;
using Next.Cqrs.Queries.MongoDb;

namespace Next.Cqrs.Configuration
{
    public static class ProjectionsBuilderExtensions
    {
        public static IMongoDbProjectionBuilder UseMongoDb(
            this IProjectionsBuilder projectionsBuilder,
            string connectionString,
            string database)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString));
            }
            
            if (string.IsNullOrWhiteSpace(database))
            {
                throw new ArgumentNullException(nameof(database));
            }
            
            return new MongoDbProjectionBuilder(
                projectionsBuilder,
                connectionString,
                database);
        }
    }
}