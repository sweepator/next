using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Next.Cqrs.Commands;
using Next.Web.Binders;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Next.Web.Application.Swagger
{
    internal class CqrsFilter: IDocumentFilter, ISchemaFilter, IParameterFilter
    {
        private readonly IOptions<MvcOptions> _mvcOptions;

        public CqrsFilter(IOptions<MvcOptions> mvcOptions)
        {
            _mvcOptions = mvcOptions;
        }
        
        private IApiRequestMetadataProvider[] GetRequestMetadataAttributes(ControllerActionDescriptor action)
        {
            if (action.FilterDescriptors == null)
            {
                return null;
            }

            // This technique for enumerating filters will intentionally ignore any filter that is an IFilterFactory
            // while searching for a filter that implements IApiRequestMetadataProvider.
            //
            // The workaround for that is to implement the metadata interface on the IFilterFactory.
            return action.FilterDescriptors
                .Select(fd => fd.Filter)
                .OfType<IApiRequestMetadataProvider>()
                .ToArray();
        }
        
        private static MediaTypeCollection GetDeclaredContentTypes(IApiRequestMetadataProvider[] requestMetadataAttributes)
        {
            // Walk through all 'filter' attributes in order, and allow each one to see or override
            // the results of the previous ones. This is similar to the execution path for content-negotiation.
            var contentTypes = new MediaTypeCollection();
            if (requestMetadataAttributes != null)
            {
                foreach (var metadataAttribute in requestMetadataAttributes)
                {
                    metadataAttribute.SetContentTypes(contentTypes);
                }
            }

            return contentTypes;
        }
        
        private IReadOnlyList<ApiRequestFormat> GetSupportedFormats(MediaTypeCollection contentTypes, Type type)
        {
            if (contentTypes.Count == 0)
            {
                contentTypes = new MediaTypeCollection
                {
                    (string)null,
                };
            }

            var results = new List<ApiRequestFormat>();
            foreach (var contentType in contentTypes)
            {
                foreach (var formatter in _mvcOptions.Value.InputFormatters)
                {
                    if (formatter is IApiRequestFormatMetadataProvider requestFormatMetadataProvider)
                    {
                        var supportedTypes = requestFormatMetadataProvider.GetSupportedContentTypes(contentType, type);

                        if (supportedTypes != null)
                        {
                            foreach (var supportedType in supportedTypes)
                            {
                                results.Add(new ApiRequestFormat()
                                {
                                    Formatter = formatter,
                                    MediaType = supportedType,
                                });
                            }
                        }
                    }
                }
            }

            return results;
        }
        
        private IEnumerable<string> InferRequestContentTypes(ApiDescription apiDescription)
        {
            // If there's content types explicitly specified via ConsumesAttribute, use them
            var explicitContentTypes = apiDescription.CustomAttributes().OfType<ConsumesAttribute>()
                .SelectMany(attr => attr.ContentTypes)
                .Distinct();
            
            if (explicitContentTypes.Any())
            {
                return explicitContentTypes;
            }

            // If there's content types surfaced by ApiExplorer, use them
            var apiExplorerContentTypes = apiDescription.SupportedRequestFormats
                .Select(format => format.MediaType)
                .Distinct();
            if (apiExplorerContentTypes.Any())
            {
                return apiExplorerContentTypes;
            }

            return Enumerable.Empty<string>();
        }
        
        public void Apply(
            OpenApiDocument swaggerDoc, 
            DocumentFilterContext context)
        {
            foreach (var apiDescription in context.ApiDescriptions)
            {
                var requestMetadataAttributes = GetRequestMetadataAttributes((ControllerActionDescriptor)apiDescription.ActionDescriptor);
                var mediaTypes = GetDeclaredContentTypes(requestMetadataAttributes);
                
                var openApiPath = swaggerDoc.Paths[$"/{apiDescription.RelativePath}"];
                var operationType = Enum.Parse<OperationType>(apiDescription.HttpMethod, true);
                var openApiOperation = openApiPath.Operations[operationType];
                
                // get parameters that corresponds to resource ids 
                var parameterDescriptionIds = apiDescription
                    .ParameterDescriptions
                    .Where(o => o.Name.EndsWith("id.value", StringComparison.InvariantCultureIgnoreCase));

                // if it already exists in the path route, then we should remove the related parameters when we are using IIdentity type
                foreach (var parameterDescriptionId in parameterDescriptionIds)
                {
                    var parameters = openApiOperation
                        .Parameters
                        .Where(o => o.Name.Equals(
                            parameterDescriptionId.Name,
                            StringComparison.InvariantCultureIgnoreCase))
                        .ToList();
                    
                    foreach (var parameter in parameters)
                    {
                        openApiOperation.Parameters.Remove(parameter);
                    }
                }

                if (apiDescription.ParameterDescriptions.Count > 0)
                {
                    foreach (var parameterDescription in apiDescription.ParameterDescriptions)
                    {
                        if (typeof(IAggregateCommand).IsAssignableFrom(parameterDescription.Type))
                        {
                            if (parameterDescription.BindingInfo != null &&
                                parameterDescription.BindingInfo.BindingSource ==
                                BodyAndRouteBindingSource.BodyAndRoute)
                            {
                                var openApiParameter = openApiOperation
                                    .Parameters
                                    .FirstOrDefault(o => o.Name.Equals(parameterDescription.Name));
                                openApiOperation.Parameters.Remove(openApiParameter);
                                
                                if (apiDescription.SupportedRequestFormats.Count == 0)
                                {
                                    var requestFormats = GetSupportedFormats(mediaTypes, parameterDescription.Type);
                                    foreach (var format in requestFormats)
                                    {
                                        apiDescription.SupportedRequestFormats.Add(format);
                                    }
                                }
                                var contentTypes = InferRequestContentTypes(apiDescription);
                                var schema = context.SchemaGenerator.GenerateSchema(
                                    parameterDescription.Type,
                                    context.SchemaRepository);

                                openApiOperation.RequestBody = new OpenApiRequestBody
                                {
                                    Content = contentTypes
                                        .ToDictionary(
                                            contentType => contentType,
                                            contentType => new OpenApiMediaType
                                            {
                                                Schema = schema
                                            }
                                        ),
                                    Required = true
                                };
                            }
                        }
                    }
                }
            }
        }

        public void Apply(
            OpenApiSchema schema, 
            SchemaFilterContext context)
        {
            if (!typeof(IAggregateCommand).IsAssignableFrom(context.Type))
            {
                return;
            }
            
            var keyId = schema.Properties.Keys.FirstOrDefault(o => o.Equals("id", StringComparison.InvariantCultureIgnoreCase));
            if (!string.IsNullOrEmpty(keyId))
            {
                schema.Properties.Remove(keyId);
            }
        }

        public void Apply(
            OpenApiParameter parameter, 
            ParameterFilterContext context)
        {
            parameter.Name = parameter.Name.ToCamelCase();
        }
    }
}