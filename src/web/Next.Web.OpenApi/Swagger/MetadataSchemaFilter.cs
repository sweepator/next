using System.Linq;
using Microsoft.OpenApi.Models;
using Next.Abstractions.Serialization.Metadata;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Next.Web.OpenApi.Swagger
{
    public class MetadataSchemaFilter : ISchemaFilter
    {
        private readonly ISerializerMetadataProvider _serializerMetadataProvider;

        public MetadataSchemaFilter(ISerializerMetadataProvider serializerMetadataProvider)
        {
            _serializerMetadataProvider = serializerMetadataProvider;
        }
        
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            var propertiesToRemove = schema.Properties
                .Keys
                .Where(key => _serializerMetadataProvider.CanExcludeProperty(context.Type, key))
                .ToList();
            
            foreach (var propertyName in propertiesToRemove)
            {
                schema.Properties.Remove(propertyName);
            }
        }
    }
}