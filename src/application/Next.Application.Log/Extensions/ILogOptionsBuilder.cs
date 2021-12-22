using Next.Application.Log;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public interface ILogOptionsBuilder
    {
        ILogOptionsBuilder Config<TRequest>(Action<LogStepOptions> setup);
    }
}
