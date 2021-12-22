using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;

namespace Next.Web.Hypermedia
{
    internal class LinkTransformationContextFactory : ILinkTransformationContextFactory
    {
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly LinkGenerator _generator;

        public LinkTransformationContextFactory(
            IActionContextAccessor actionContextAccessor, 
            LinkGenerator generator)
        {
            _actionContextAccessor = actionContextAccessor;
            _generator = generator;
        }
        
        public LinkTransformationContext CreateContext(ILinkSpec spec)
        {
            return new (spec, 
                _actionContextAccessor.ActionContext, 
                _generator);
        }
    }
}