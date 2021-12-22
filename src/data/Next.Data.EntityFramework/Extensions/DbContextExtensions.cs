using Next.Data.EntityFramework.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.EntityFrameworkCore
{
    public static class DbContextExtensions
    {
        public static void Merge<TEntity>(this DbContext dbContext,
            IEnumerable<TEntity> entities,
            Action<TEntity, TEntity> updateAction)
            where TEntity : class, IEntity
        {
            var deletedEntries = dbContext
                .ChangeTracker
                .Entries<TEntity>()
                .Where(o => !entities.Select(e => e.Id).Contains(o.Property(nameof(IEntity.Id)).CurrentValue))
                .ToList();

            deletedEntries.ForEach(o =>
            {
                o.State = EntityState.Deleted;
            });

            entities
                .Where(e => !deletedEntries.Select(de => de.Entity.Id).Contains(e.Id))
                .ToList()
                .ForEach(async e => 
                {
                    var original = await dbContext.FindAsync<TEntity>(e.Id);

                    if (original != null)
                    {
                        updateAction?.Invoke(e, original);
                    }
                    else
                    {
                        await dbContext.Set<TEntity>().AddAsync(e);
                    }
                });
        }
    }
}
