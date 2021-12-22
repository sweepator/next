using Microsoft.Extensions.DependencyInjection;
using Serilog.Core;
using Serilog.Events;
using System;

namespace Next.Log.Serilog.Enrichers
{
    internal class ServiceProviderEnricher : ILogEventEnricher
    {
        private readonly IServiceProvider _serviceProvider;

        internal ServiceProviderEnricher(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var enrichers = scope.ServiceProvider?.GetServices<ILogEventEnricher>();

                if (enrichers != null)
                {
                    foreach (var enricher in enrichers)
                    {
                        enricher.Enrich(logEvent, propertyFactory);
                    }
                }
            }
        }
    }
}
