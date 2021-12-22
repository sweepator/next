namespace Next.Abstractions.EventSourcing.Snapshot
{
    public class SnapshotProcessorOptions
    {
        public int BackgroundLockInSeconds { get; set; } = 10;
        public int BackgroundLockOnErrorInSeconds { get; set; } = 10;
    }
}