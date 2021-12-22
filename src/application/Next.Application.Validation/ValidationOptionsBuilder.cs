using System;
using Microsoft.Extensions.DependencyInjection;

namespace Next.Application.Validation
{
    internal sealed class ValidationOptionsBuilder : IValidationOptionsBuilder
    {
        private readonly IServiceCollection _services;

        internal ValidationOptionsBuilder(IServiceCollection services)
        {
            _services = services;
        }

        public IValidationOptionsBuilder Config<TRequest>(Action<ValidationStepOptions> setup)
        {
            if (setup == null)
            {
                throw new ArgumentNullException(nameof(setup));
            }

            _services
                .AddOptions<ValidationStepOptions>(typeof(TRequest).Name)
                .Configure(setup);

            return this;
        }
    }
}
