using System.Collections;

namespace Next.Application.Pipelines
{
    public sealed class OperationContext : IOperationContext
    {
        //private static readonly OperationContextAccessor OperationContextAccessor = new();

        public IFeatureCollection Features { get; } = new FeatureCollection();

        public OperationContext()
        {
            //OperationContextAccessor.Context = this;
        }

        public void Dispose()
        {
            //OperationContextAccessor.Context = null;
        }
    }
}
