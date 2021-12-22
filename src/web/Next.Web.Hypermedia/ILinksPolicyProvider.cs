using System;

namespace Next.Web.Hypermedia
{
    public interface ILinksPolicyProvider
    {
        ILinksPolicy GetPolicy<TResource>();
        ILinksPolicy GetPolicy<TResource>(string name);
        ILinksPolicy GetPolicy(Type resourceType);
        public ILinksPolicy GetPolicy(
            Type resourceType,
            string name);
    }
}