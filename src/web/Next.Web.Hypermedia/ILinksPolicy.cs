using System.Collections.Generic;

namespace Next.Web.Hypermedia
{
    public interface ILinksPolicy
    {
        IReadOnlyList<ILinksRequirement> Requirements { get; }
    }
}