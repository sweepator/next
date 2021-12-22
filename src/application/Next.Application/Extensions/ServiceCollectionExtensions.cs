using System;
using System.Collections.Generic;
using System.Linq;
using Next.Application.Pipelines;
using MediatR;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationPipeline(
            this IServiceCollection services,
            params Assembly[] assemblies)
        {
            if (assemblies == null)
            {
                throw new ArgumentNullException(nameof(assemblies));
            }
            
            services.AddMediatR(assemblies);
            services.TryAddSingleton<IPipelineEngine, PipelineEngine>();

            var operationContextAccessor = OperationContextAccessor.Instance;
            services.TryAddSingleton<IOperationContextAccessor>(operationContextAccessor);
            services.TryAddSingleton(operationContextAccessor);
            return services;
        }
    }
}
