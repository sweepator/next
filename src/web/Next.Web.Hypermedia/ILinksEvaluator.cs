using System.Collections.Generic;

namespace Next.Web.Hypermedia
{
    public interface ILinksEvaluator
    {
        void BuildLinks(
            IEnumerable<ILinkSpec> linkSpecs,
            ILinkedResource resource);
    }
}