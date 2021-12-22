using Next.Data.EntityFramework.Model;
using Next.Abstractions.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Next.Data.EntityFramework
{
    public abstract class BaseDbContext : DbContext, IDataMigrations
    {
        public BaseDbContext(DbContextOptions options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder
                .Model
                .GetEntityTypes()
                .Where(o => typeof(ISoftDeletableEntity).IsAssignableFrom(o.ClrType))
                .Select(o => o.ClrType)
                .ToList()
                .ForEach(entityType =>
                {
                    var method = typeof(ModelBuilder)
                        .GetMethods()
                        .Single(m => m.Name == "Entity" && m.IsGenericMethodDefinition && !m.GetParameters().Any());

                    var entityTypeBuilder = method
                        .MakeGenericMethod(entityType)
                        .Invoke(modelBuilder, null);

                    typeof(BaseDbContext)
                        .GetMethod(nameof(ApplySoftDeleteConfiguration), BindingFlags.Instance | BindingFlags.NonPublic)
                        ?.MakeGenericMethod(entityType)
                        .Invoke(this, new object[] { entityTypeBuilder });
                });
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, 
            CancellationToken cancellationToken = default)
        {
            UpdateDates();
            HandleSoftDelete();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        private void HandleSoftDelete()
        {
            ChangeTracker.DetectChanges();

            var markedAsDeleted = ChangeTracker
                .Entries()
                .Where(e => e.State == EntityState.Deleted);

            foreach (var item in markedAsDeleted)
            {
                if (item.Entity is ISoftDeletableEntity entity)
                {
                    item.State = EntityState.Unchanged;
                    entity.IsDeleted = true;
                }
            }
        }

        private void UpdateDates()
        {
            var entries = ChangeTracker
                .Entries()
                .Where(e =>( e.Entity is ITimestampedEntity) && (e.State == EntityState.Added || e.State == EntityState.Modified));

            foreach (var entityEntry in entries)
            {
                if (entityEntry.Entity is ITimestampedEntity entity)
                {
                    var now = DateTime.UtcNow;
                    entity.UpdatedDate = now;

                    if (entityEntry.State == EntityState.Added)
                    {
                        entity.CreatedDate = now;
                    }
                }
            }
        }

        private void ApplySoftDeleteConfiguration<TEntity>(EntityTypeBuilder<TEntity> entityTypeBuilder)
            where TEntity : class, ISoftDeletableEntity
        {
            entityTypeBuilder.HasQueryFilter(o => !o.IsDeleted);
        }

        public void Migrate()
        {
            Database.Migrate();
        }

        public async Task MigrateAsync()
        {
            await Database.MigrateAsync();
        }

        public Task<DataMigration> GetLastMigrationAsync()
        {
            throw new NotImplementedException();
        }
    }
}
