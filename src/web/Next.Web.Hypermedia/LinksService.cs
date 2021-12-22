using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Next.Web.Hypermedia
{
    public class LinksService : ILinksService
    {
        private readonly IEnumerable<ILinksHandler> _handlers;
        private readonly ILinksPolicyProvider _policyProvider;
        private readonly IHateoasHandlerContextFactory _hateoasHandlerContextFactory;
        private readonly ILinksEvaluator _evaluator;
        private readonly IHateoasRouteMap _hateoasRouteMap;

        public LinksService(
            IEnumerable<ILinksHandler> handlers,
            ILinksPolicyProvider policyProvider,
            IHateoasHandlerContextFactory hateoasHandlerContextFactory,
            ILinksEvaluator evaluator,
            IHateoasRouteMap hateoasRouteMap)
        {
            _handlers = handlers;
            _policyProvider = policyProvider;
            _hateoasHandlerContextFactory = hateoasHandlerContextFactory;
            _evaluator = evaluator;
            _hateoasRouteMap = hateoasRouteMap;
        }
        
        public async Task AddLinks<TResource>(TResource linkedResource) 
            where TResource : ILinkedResource
        {
            switch (linkedResource)
            {
                case ILinkedResourceList linkedResourceList:
                {
                    foreach (var linkedResourceItem in linkedResourceList.Content)
                    {
                        await AddLinks(linkedResourceItem);
                    }
                } break;
            }

            var currentRouteName = _hateoasRouteMap.GetCurrentRoute().RouteName;
            var resource = linkedResource.GetResource();
            var policy = _policyProvider.GetPolicy(resource.GetType(), currentRouteName) ?? 
                     _policyProvider.GetPolicy(resource.GetType());
            
            if (policy != null)
            {
                var context = _hateoasHandlerContextFactory.CreateContext(
                    policy.Requirements, 
                    resource);
                
                foreach (var handler in _handlers)
                {
                    await handler.HandleAsync(context);
                }
                
                _evaluator.BuildLinks(context.Links, linkedResource);
            }
        }
    }
}