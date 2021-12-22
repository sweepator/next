using System.Threading.Tasks;

namespace Next.Web.Hypermedia
{
    public class SelfLinkRequirement<TResource> : LinksHandler<SelfLinkRequirement<TResource>>, ILinksRequirement
    {
        private const string Self = "self";
        
        protected override Task HandleRequirementAsync(
            HateoasHandlerContext context, 
            SelfLinkRequirement<TResource> requirement)
        {
            var route = context.CurrentRoute;
            var values = context.CurrentRouteValues;
            context.AddLink(new LinkSpec(
                Self, 
                route, 
                values));
            
            return Task.CompletedTask;
        }
    }
}