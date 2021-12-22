using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Next.Web.OpenApi.Swagger
{
    public class AuthorizeCheckOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var hasAuthorisation = context.MethodInfo.DeclaringType != null &&
                                   (context.MethodInfo.DeclaringType.GetCustomAttributes(true)
                                        .OfType<AuthorizeAttribute>()
                                        .Any() ||
                                    context.MethodInfo.GetCustomAttributes(true)
                                        .OfType<AuthorizeAttribute>()
                                        .Any());

            if (hasAuthorisation)
            {
                operation.Responses.Add("401", new OpenApiResponse { Description = "Unauthorized" });
                operation.Responses.Add("403", new OpenApiResponse { Description = "Forbidden" });

                operation.Security = new List<OpenApiSecurityRequirement>
                {
                    new ()
                    {
                        [
                                new OpenApiSecurityScheme
                                {
                                    Reference = new OpenApiReference
                                    {
                                        Type = ReferenceType.SecurityScheme,
                                        Id = "oauth2"
                                    }
                                }

                            ]
                            = new[] { "api1" }
                    }
                };
            }
        }
    }
}