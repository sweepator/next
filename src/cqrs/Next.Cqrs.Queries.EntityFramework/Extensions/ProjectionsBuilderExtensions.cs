using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Next.Cqrs.Queries.EntityFramework;

namespace Next.Cqrs.Configuration
{
    public static class ProjectionsBuilderExtensions
    {
        public static IEntityFrameworkProjectionBuilder UseEntityFramework<TDbContext>(this IProjectionsBuilder projectionsBuilder)
            where TDbContext : DbContext
        {
            return new EntityFrameworkProjectionBuilder<TDbContext>(projectionsBuilder);
        }

        public static IEntityFrameworkProjectionBuilder UseEntityFramework(this IProjectionsBuilder projectionsBuilder)
        {
            return new EntityFrameworkProjectionBuilder<QueryStoreDbContext>(projectionsBuilder);
            
        }
        
        public static IProjectionsBuilder AddQueryStoreDbContext(
            this IProjectionsBuilder projectionsBuilder,
            Action<DbContextOptionsBuilder> setup)
        {
            return projectionsBuilder
                .AddQueryStoreDbContext<QueryStoreDbContext>(setup);
        }

        public static IProjectionsBuilder AddQueryStoreDbContext<TDbContext>(
            this IProjectionsBuilder projectionsBuilder,
            Action<DbContextOptionsBuilder> setup)
            where TDbContext : QueryStoreDbContext
        {
            if (setup == null)
            {
                throw new ArgumentNullException(nameof(setup));
            }
            
            projectionsBuilder
                .Services
                .AddDbContextFactory<TDbContext>(setup)
                .AddTransient(o => o
                    .GetRequiredService<IDbContextFactory<TDbContext>>()
                    .CreateDbContext());

            return projectionsBuilder;
        }
    }
}