using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Next.Web.Hypermedia
{
    public class LinkTransformationContext
    {
        public virtual ILinkSpec LinkSpec { get; }
        public ActionContext ActionContext { get; }
        public HttpContext HttpContext => ActionContext.HttpContext;
        public RouteValueDictionary RouteValues => ActionContext.RouteData.Values;
        public LinkGenerator LinkGenerator { get; }

        public LinkTransformationContext(ILinkSpec spec, 
            ActionContext actionContext, 
            LinkGenerator linkGenerator)
        {
            LinkSpec = spec;
            ActionContext = actionContext;
            LinkGenerator = linkGenerator;
        }
    }
}