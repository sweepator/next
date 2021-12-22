using System.Linq;
using System.Threading.Tasks;

namespace Next.Web.Hypermedia
{
    public abstract class LinksHandler<TRequirement> : ILinksHandler
        where TRequirement : ILinksRequirement
    {

        public async Task HandleAsync(HateoasHandlerContext context)
        {            
            foreach (var req in context.Requirements.OfType<TRequirement>())
            {
                await HandleRequirementAsync(context, req);
            }
        }

        protected abstract Task HandleRequirementAsync(HateoasHandlerContext context, TRequirement requirement);
    }
}