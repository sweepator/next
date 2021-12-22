using System;
using HotChocolate.Types;
using Next.Cqrs.Queries.Projections;

namespace Next.Web.Application.Graphql
{
    public class ProjectionType<TProjection> : ObjectType<TProjection>
        where TProjection: IProjectionModel
    {
        protected override void Configure(IObjectTypeDescriptor<TProjection> descriptor)
        {
            var projectionName = typeof(TProjection).Name.Replace(
                "Projection",
                string.Empty,
                StringComparison.InvariantCultureIgnoreCase)
                .ToLower();
                
            descriptor.BindFieldsImplicitly();
            descriptor.Name(projectionName);
        }
    }
}