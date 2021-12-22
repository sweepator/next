using System.Collections.Generic;

namespace Next.Web.Hypermedia
{
    public interface ILinkedResourceList : ILinkedResource
    {
        IEnumerable<ContentLinkedResource> Content { get; }
    }
}