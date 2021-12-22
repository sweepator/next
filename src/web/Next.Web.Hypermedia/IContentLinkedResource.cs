using System.Collections;

namespace Next.Web.Hypermedia
{
    public interface IContentLinkedResource : ILinkedResource
    {
        object Content { get; }
    }

    public interface IContentLinkedResource<out TContent> : IContentLinkedResource
    {
        new TContent Content { get; }
        
        object IContentLinkedResource.Content => Content;
    }
}