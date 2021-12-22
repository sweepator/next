using System.Collections.Concurrent;

namespace Next.Abstractions.EventSourcing.Snapshot
{
    public class SnapshotProcessor : ISnapshotProcessor
    {
        private static readonly ConcurrentQueue<ISnapshot> Queue = new();
        
        public ISnapshot GetNextSnapshot()
        {
            return Queue.TryDequeue(out var snapshot) ? snapshot : null;
        }

        public void AddSnapshot(ISnapshot snapshot)
        {
            Queue.Enqueue(snapshot);
        }

        public void AbortSnapshot(ISnapshot snapshot)
        {
            Queue.Enqueue(snapshot);
        }
    }
}