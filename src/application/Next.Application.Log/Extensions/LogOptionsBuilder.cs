using Next.Application.Log;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    internal sealed class LogOptionsBuilder : ILogOptionsBuilder
    {
        private readonly IServiceCollection _services;

        internal LogOptionsBuilder(IServiceCollection services)
        {
            _services = services;
        }

        public ILogOptionsBuilder Config<TRequest>(Action<LogStepOptions> setup)
        {
            if (setup == null)
            {
                throw new ArgumentNullException(nameof(setup));
            }

            _services
                .AddOptions<LogStepOptions>(typeof(TRequest).Name)
                .Configure(setup);

            return this;
        }
    }
}
