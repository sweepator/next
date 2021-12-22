namespace Next.Application.Pipelines
{
    public interface IOperationContextAccessor
    {
        IOperationContext Context { get; }
    }
}
