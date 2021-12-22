using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace Next.Web.Hypermedia
{
    public class LinksPolicyProvider : ILinksPolicyProvider
    {
        private readonly LinksOptions _options;

        public LinksPolicyProvider(IOptions<LinksOptions> options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            _options = options.Value;
        }

        public ILinksPolicy GetPolicy<TResource>()
        {
            return _options.GetPolicy<TResource>();
        }

        public ILinksPolicy GetPolicy<TResource>(string name)
        {
            return _options.GetPolicy<TResource>(name);
        }

        public ILinksPolicy GetPolicy(Type resourceType)
        {
            return _options.GetPolicy(resourceType);
        }
        
        public ILinksPolicy GetPolicy(
            Type resourceType,
            string name)
        {
            return _options.GetPolicy(
                resourceType,
                name);
        }
    }
}