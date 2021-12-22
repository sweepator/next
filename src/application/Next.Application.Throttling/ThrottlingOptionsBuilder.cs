using System;
using Microsoft.Extensions.DependencyInjection;

namespace Next.Application.Throttling
{
    internal sealed class ThrottlingOptionsBuilder : IThrottlingOptionsBuilder
    {
        private readonly IServiceCollection _services;

        internal ThrottlingOptionsBuilder(IServiceCollection services)
        {
            _services = services;
        }

        public IThrottlingOptionsBuilder Config<TRequest>(Action<ThrottlingStepOptions> setup)
        {
            if (setup == null)
            {
                throw new ArgumentNullException(nameof(setup));
            }

            _services
                .AddOptions<ThrottlingStepOptions>(typeof(TRequest).Name)
                .Configure(setup);

            return this;
        }
    }
}
