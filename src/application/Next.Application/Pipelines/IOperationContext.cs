using System;

namespace Next.Application.Pipelines
{
    public interface IOperationContext: IDisposable
    {
        IFeatureCollection Features { get; }
    }
}
