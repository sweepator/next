using Next.Log.Serilog.Enrichers;
using System;

namespace Serilog.Configuration
{
    public static class SerilogConfigurationExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        public static LoggerConfiguration WithServiceProvider(this LoggerEnrichmentConfiguration configuration,
            IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
            {
                throw new ArgumentNullException(nameof(serviceProvider));
            }

            return configuration.With(new ServiceProviderEnricher(serviceProvider));
        }
    }
}
