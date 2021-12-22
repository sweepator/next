namespace Next.Abstractions.EventSourcing.Snapshot
{
    public interface ISnapshotProcessor
    {
        ISnapshot GetNextSnapshot();
        void AddSnapshot(ISnapshot snapshot);
        void AbortSnapshot(ISnapshot snapshot);
    }
}