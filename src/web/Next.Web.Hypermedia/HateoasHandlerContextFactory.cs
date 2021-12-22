using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace Next.Web.Hypermedia
{
    internal class HateoasHandlerContextFactory : IHateoasHandlerContextFactory
    {
        private readonly IHateoasRouteMap _routeMap;
        private readonly IActionContextAccessor _actionContextAccessor;

        public HateoasHandlerContextFactory(
            IHateoasRouteMap routeMap,
            IActionContextAccessor actionAccessor)
        {
            _routeMap = routeMap;
            _actionContextAccessor = actionAccessor;
        }
        
        public HateoasHandlerContext CreateContext(
            IEnumerable<ILinksRequirement> requirements, 
            object resource)
        {
            return new(
                requirements,
                _routeMap, 
                _actionContextAccessor.ActionContext,
                resource);
        }
    }
}