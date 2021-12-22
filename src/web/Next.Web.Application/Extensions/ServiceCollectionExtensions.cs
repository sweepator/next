using System;
using Next.Web.Application.Error;
using Next.Web.Application.PortAdapters;
using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Next.Application.Pipelines;
using Next.Web.Application.Pipelines;
using ProblemDetailsFactory = Next.Web.Application.Error.ProblemDetailsFactory;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Add http application pipeline port adapter services
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddHttpPortAdapter(this IServiceCollection services)
        {
            services.AddScoped(typeof(MediatR.IPipelineBehavior<,>), typeof(HttpContextBinderStep<,>));
            services.TryAddSingleton<IProblemDetailsFactory, ProblemDetailsFactory>();
            return services.AddScoped<IHttpPortAdapter, HttpPortAdapter>();
        }
        
        public static IServiceCollection AddProblemDetailsDefaults(
            this IServiceCollection services,
            Action<IProblemDetailsBuilder> setup = null)
        {
            var problemDetailsBuilder = new ProblemDetailsBuilder(services);
            setup?.Invoke(problemDetailsBuilder);
            
            services
                .AddProblemDetails(o =>
                {
                    o.GetTraceId = (httpContext) => httpContext.GetRequestId();
                    o.Map<Exception>((httpContext, ex)=>
                    {
                        var problemDetailsFactory = (IProblemDetailsFactory) httpContext
                            .RequestServices
                            .GetService(typeof(IProblemDetailsFactory));
                        return problemDetailsFactory?.Create(ex);
                    });
                });   
            
            services
                .TryAddSingleton<IProblemDetailsFactory, ProblemDetailsFactory>();

            return services
                .AddSingleton<IProblemDetailsProfile, ProblemDetailsProfileDefaultConventions>()
                .AddSingleton<IProblemDetailsProfile, ProblemDetailsValidationProfile>();
        }
    }
}