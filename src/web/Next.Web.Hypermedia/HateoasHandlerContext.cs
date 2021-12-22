using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Next.Web.Hypermedia
{
    public class HateoasHandlerContext
    {
        private readonly List<ILinkSpec> _linkSpecs = new();
        
        public ActionContext ActionContext { get; }

        public object Resource { get; }

        public IEnumerable<ILinksRequirement> Requirements { get; }
        
        public virtual IHateoasRouteMap RouteMap { get; }

        public ClaimsPrincipal User => ActionContext?.HttpContext?.User;

        public RouteInfo CurrentRoute => RouteMap.GetCurrentRoute();
        
        public RouteValueDictionary CurrentRouteValues => ActionContext?.RouteData?.Values;
        
        public IQueryCollection CurrentQueryValues => ActionContext?.HttpContext?.Request?.Query ?? new QueryCollection();

        public virtual IReadOnlyCollection<ILinkSpec> Links => _linkSpecs;
        
        public HateoasHandlerContext(
            IEnumerable<ILinksRequirement> requirements,
            IHateoasRouteMap routeMap,
            ActionContext actionContext,
            object resource)
        {
            Requirements = requirements;
            RouteMap = routeMap;           
            ActionContext = actionContext ?? throw new ArgumentNullException(nameof(actionContext));
            Resource = resource ?? throw new ArgumentNullException(nameof(resource));
        }
        
        public bool AssertAll<TResource>(LinkCondition<TResource> condition)
        {
            if (condition == null)
            {
                throw new ArgumentNullException(nameof(condition));
            }
            return !condition.Assertions.Any() || condition
                .Assertions
                .All(a => a(
                    (TResource)Resource,
                    ActionContext.HttpContext));
        }

        public void AddLink(ILinkSpec linkSpec)
        {
            if (Links.All(o => o.Id != linkSpec.Id))
            {
                _linkSpecs.Add(linkSpec);
            }
        }
    }
}