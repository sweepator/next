using System;
using HotChocolate.Types;
using Next.Cqrs.Queries;
using Next.Cqrs.Queries.Projections;

namespace Next.Web.Application.Graphql
{
    public class QueryRequestType<TQueryRequest, TProjection> : InputObjectType<TQueryRequest>
        where TQueryRequest: IQueryRequest<TProjection>
        where TProjection: IProjectionModel
    {
        protected override void Configure(IInputObjectTypeDescriptor<TQueryRequest> descriptor)
        {
            descriptor.BindFieldsImplicitly();
            var queryRequestName = typeof(TQueryRequest).Name.Replace(
                "Request",
                string.Empty,
                StringComparison.InvariantCultureIgnoreCase)
                .ToLower();
            descriptor.Name(queryRequestName);
        }
    }
}