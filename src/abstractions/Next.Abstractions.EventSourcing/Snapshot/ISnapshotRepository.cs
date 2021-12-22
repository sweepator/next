using System.Threading.Tasks;
using Next.Abstractions.Domain;

namespace Next.Abstractions.EventSourcing.Snapshot
{
    public interface ISnapshotRepository
    {
        Task<ISerializedSnapshot> GetSnapshot(IIdentity id);
        
        Task SaveSnapshot(ISerializedSnapshot snapshot);
    }
}