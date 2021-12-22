using System;
using System.Collections.Generic;

namespace Next.Web.Hypermedia
{
    public interface ILinkedResource
    {
        IEnumerable<Link> Links { get; }
        
        void AddLink(Link link);
        object GetResource();
    }
}