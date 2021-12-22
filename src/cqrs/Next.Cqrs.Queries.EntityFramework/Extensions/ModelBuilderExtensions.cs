using System;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Next.Cqrs.Queries.Projections;

namespace Microsoft.EntityFrameworkCore
{
    public static class ModelBuilderExtensions
    {
        public static ModelBuilder AddProjectionModel<TProjectionModel>(
            this ModelBuilder modelBuilder,
            Action<EntityTypeBuilder<TProjectionModel>> setup = null)
            where TProjectionModel : class, IProjectionModel
        {
            var type = typeof(TProjectionModel);
            var entityTypeBuilder = modelBuilder.Entity<TProjectionModel>();
            entityTypeBuilder.ToTable(GetDefaultTableName(type));
            
            setup?.Invoke(entityTypeBuilder);
            return modelBuilder;
        }
        
        public static ModelBuilder AddProjectionModels(
            this ModelBuilder modelBuilder,
            IProjectionModelDefinitionService projectionModelDefinitionService)
        {
            foreach (var projectionModelDefinition in projectionModelDefinitionService.GetAllDefinitions())
            {
                var type = projectionModelDefinition.Type;
                var entityTypeBuilder = modelBuilder.Entity(type);
                entityTypeBuilder.ToTable(GetDefaultTableName(type));
                return modelBuilder;
            }

            return modelBuilder;
        }

        private static string GetDefaultTableName(Type type)
        {
            var table = $"Projection-{type.Name.Replace("Projection", string.Empty)}";
            return table;
        }
    }
}