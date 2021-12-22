using System;
using System.Linq;
using System.Reflection;
using HotChocolate.Types;
using Next.Cqrs.Configuration;
using Next.Cqrs.Queries;
using Next.Cqrs.Queries.Projections;

namespace Next.Web.Application.Graphql
{
    public class QueryType: ObjectType
    {
        private readonly IDomainMetadataInfo _domainMetadataInfo;
            
        private static readonly MethodInfo AddQueryProjectionDescriptorMethod =
            typeof(QueryType)
                .GetTypeInfo()
                .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                .Single(m => m.Name == nameof(AddQueryProjectionDescriptor) && m.GetGenericArguments().Length == 2);

        public QueryType(IDomainMetadataInfo domainMetadataInfo)
        {
            _domainMetadataInfo = domainMetadataInfo;
        }

        private void AddQueryProjectionDescriptor<TQueryRequest, TProjection>(IObjectTypeDescriptor descriptor)
            where TQueryRequest:IQueryRequest<TProjection>
            where TProjection:IProjectionModel
        {
            var projectionName = typeof(TProjection).Name.Replace(
                "Projection",
                string.Empty,
                StringComparison.InvariantCultureIgnoreCase)
                .ToLower();

            descriptor
                .Field(projectionName)
                .Name(projectionName)
                .Type<ListType<ProjectionType<TProjection>>>()
                .Argument("input", a => a
                    .Type<NonNullType<QueryRequestType<TQueryRequest, TProjection>>>())
                .ResolveWith<QueryResolvers<TQueryRequest, TProjection>>(r =>
                    r.Execute(default, default, default));
        }
            
        protected override void Configure(IObjectTypeDescriptor descriptor)
        {
            descriptor.Name("query");

            foreach (var projectionType in _domainMetadataInfo.QueryRequestByProjectionTypes.Keys)
            {
                if (_domainMetadataInfo.QueryRequestByProjectionTypes[projectionType].Count() > 1)
                {
                    throw new NotImplementedException();
                }

                var queryRequestType = _domainMetadataInfo.QueryRequestByProjectionTypes[projectionType].SingleOrDefault();

                if (queryRequestType != null)
                {
                    var method = AddQueryProjectionDescriptorMethod.MakeGenericMethod(
                        queryRequestType,
                        projectionType);

                    method.Invoke(this, new object[] { descriptor });
                }
            }
        }
    }
}