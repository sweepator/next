namespace Next.Abstractions.Jobs
{
    public interface IJobContextAccessor
    {
        IJobContext Context { get; set; }
    }
}
