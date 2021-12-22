using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Next.Web.Hypermedia
{
    public static class LinksPolicyBuilderExtensions
    {
        public static LinksPolicyBuilder<TResource> RequireSelfLink<TResource>(this LinksPolicyBuilder<TResource> builder)
        {
            return builder.Requires(new SelfLinkRequirement<TResource>());
        }

        public static LinksPolicyBuilder<TResource> RequireRoutedLink<TResource>(
            this LinksPolicyBuilder<TResource> builder,
            string id,
            string routeName,
            Func<TResource, HttpContext, object> getValues = null,
            Func<TResource, HttpContext, object> getParameters = null,
            Action<LinkConditionBuilder<TResource>> configureCondition = null)
        {
            Func<TResource, HttpContext, RouteValueDictionary> getRouteValues = (r,h) => new RouteValueDictionary();
            if (getValues != null)
            {
                getRouteValues = (r, h) => new RouteValueDictionary(getValues(r, h));
            }
            
            var conditionBuilder = new LinkConditionBuilder<TResource>();
            configureCondition?.Invoke(conditionBuilder);

            var req = new RouteLinkRequirement<TResource>()
            {
                Id = id,
                RouteName = routeName,
                GetRouteValues = getRouteValues,
                GetParameters = getParameters,
                Condition = conditionBuilder?.Build() ?? LinkCondition<TResource>.None
            };
            
            return builder.Requires(req);
        }

        /*public static LinksPolicyBuilder<TResource> RequiresPagingLinks<TResource>(this LinksPolicyBuilder<TResource> builder,
            LinkCondition<TResource> condition = null)
        {
            var req = new PagingLinksRequirement<TResource>()
            {
                Condition = condition ?? LinkCondition<TResource>.None
            };
            
            return builder.Requires(req);
        }*/
    }
}