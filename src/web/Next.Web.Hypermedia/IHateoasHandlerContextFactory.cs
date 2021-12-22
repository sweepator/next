using System.Collections.Generic;

namespace Next.Web.Hypermedia
{
    public interface IHateoasHandlerContextFactory
    {
        HateoasHandlerContext CreateContext(
            IEnumerable<ILinksRequirement> requirements, 
            object resource);
    }
}