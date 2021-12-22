using System.Threading.Tasks;

namespace Next.Web.Hypermedia
{
    public interface ILinksService
    {
        Task AddLinks<TResource>(TResource linkedResource) 
            where TResource : ILinkedResource;
    }
}