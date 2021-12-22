using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Next.Web.Hypermedia
{
    public class RouteLinkRequirement<TResource> : LinksHandler<RouteLinkRequirement<TResource>>, ILinksRequirement
    {
        public string Id { get; set; }
        public string RouteName { get; set; }
        public Func<TResource, HttpContext, RouteValueDictionary> GetRouteValues { get; set; }
        public Func<TResource, HttpContext, object> GetParameters{ get; set; }
        public LinkCondition<TResource> Condition { get; set; } = LinkCondition<TResource>.None;
        
        protected override Task HandleRequirementAsync(
            HateoasHandlerContext context, 
            RouteLinkRequirement<TResource> requirement)
        {
            var condition = requirement.Condition;
            if (!context.AssertAll(condition))
            {
                return Task.CompletedTask;
            }
            
            if(string.IsNullOrEmpty(requirement.RouteName))
            {
                throw new InvalidOperationException($"Invalid route specified in link specification.");
            }

            var route = context.RouteMap.GetRoute(requirement.RouteName);
            if (route == null)
            {
                throw new InvalidOperationException($"Invalid route specified in link specification.");
            }

            var resource = (TResource) context.Resource;
            
            var values = new RouteValueDictionary();
            if (requirement.GetRouteValues != null)
            {
                values = requirement.GetRouteValues(
                    resource, 
                    context.ActionContext.HttpContext);
            }

            object parameters = null;
            if (requirement.GetParameters != null)
            {
                parameters = requirement.GetParameters(
                    resource,
                    context.ActionContext.HttpContext);
            }

            context.AddLink(new LinkSpec(
                requirement.Id, 
                route, 
                values, 
                parameters));
            
            return Task.CompletedTask;
        }
    }
}