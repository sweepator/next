using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Next.Jobs.Hangfire
{
    public sealed class ServiceProviderJobActivator : JobActivator
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public ServiceProviderJobActivator(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
        }

        public override JobActivatorScope BeginScope(JobActivatorContext context)
        {
            return new ServiceJobActivatorScope(_serviceScopeFactory.CreateScope());
        }

        private class ServiceJobActivatorScope : JobActivatorScope
        {
            private readonly IServiceScope _serviceScope;
            
            internal ServiceJobActivatorScope(IServiceScope serviceScope)
            {
                _serviceScope = serviceScope ?? throw new ArgumentNullException(nameof(serviceScope));
            }

            public override object Resolve(Type type)
            {
                return _serviceScope.ServiceProvider.GetService(type);
            }
        }
    }
}
