using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Next.Abstractions.Serialization.Metadata;
using Next.Web.OpenApi.Swagger;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public static class SwaggerGenOptionsExtensions
    {
        public static SwaggerGenOptions AddMetadataProvider(
            this SwaggerGenOptions swaggerGenOptions,
            ISerializerMetadataProvider serializerMetadataProvider)
        {
            swaggerGenOptions.SchemaFilter<MetadataSchemaFilter>(serializerMetadataProvider);
            return swaggerGenOptions;
        }
        
        /// <summary>
        /// Add OpenOAuth configurations to be supported in swagger
        /// </summary>
        /// <param name="options"></param>
        /// <param name="host"></param>
        /// <param name="clientId"></param>
        /// <param name="clientSecret"></param>
        /// <param name="scopes"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static SwaggerGenOptions AddOpenOAuthConfiguration(
            this SwaggerGenOptions options,
            string host,
            string clientId,
            string clientSecret,
            IDictionary<string, string> scopes)
        {
            if (string.IsNullOrWhiteSpace(host))
            {
                throw new ArgumentNullException(nameof(host));
            }
            
            if (string.IsNullOrWhiteSpace(clientId))
            {
                throw new ArgumentNullException(nameof(clientId));
            }
            
            if (string.IsNullOrWhiteSpace(clientSecret))
            {
                throw new ArgumentNullException(nameof(clientSecret));
            }
            
            if (scopes == null)
            {
                throw new ArgumentNullException(nameof(scopes));
            }
            
            options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.OAuth2,
                Flows = new OpenApiOAuthFlows
                {
                    AuthorizationCode = new OpenApiOAuthFlow
                    {
                        AuthorizationUrl = new Uri($"{host}/connect/authorize"),
                        TokenUrl = new Uri($"{host}/connect/token"),
                        Scopes = scopes
                    }
                }
            });

            options.OperationFilter<AuthorizeCheckOperationFilter>();
            return options;
        }
    }
}