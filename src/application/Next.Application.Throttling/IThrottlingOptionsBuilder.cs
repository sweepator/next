using System;

namespace Next.Application.Throttling
{
    public interface IThrottlingOptionsBuilder
    {
        IThrottlingOptionsBuilder Config<TRequest>(Action<ThrottlingStepOptions> setup);
    }
}
