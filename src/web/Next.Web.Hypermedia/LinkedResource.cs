using System;
using System.Collections;
using System.Collections.Generic;

namespace Next.Web.Hypermedia
{
    public abstract class LinkedResource : ILinkedResource
    {
        private IDictionary<string, Link> _links;

        public IEnumerable<Link> Links => _links?.Values;

        public void AddLink(Link link)
        {
            _links ??= new Dictionary<string, Link>();
            _links.Add(link.Rel, link);
        }

        public abstract object GetResource();
    }

    public static class LinkedResourceFactory
    {
        public static ILinkedResource CreateLinkedResource(object resource)
        {
            if (resource is IEnumerable resourceList)
            {
                return new LinkedResourceList(resourceList);
            }
            
            return new ContentLinkedResource(resource);
        }
    }
}