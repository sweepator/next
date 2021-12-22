using System.Net.Http;
using Microsoft.AspNetCore.Routing;

namespace Next.Web.Hypermedia
{
    public struct LinkSpec : ILinkSpec
    {
        public string Id { get;  }
        public RouteInfo RouteInfo { get; }
        public RouteValueDictionary RouteValues { get; }
        public HttpMethod HttpMethod => RouteInfo?.HttpMethod;
        public object Parameters { get; }
        public bool External { get; }
        public string RouteName => RouteInfo?.RouteName;
        public string ControllerName => RouteInfo.ControllerName;
        
        public LinkSpec(string id, 
            RouteInfo routeInfo,
            RouteValueDictionary routeValues = null,
            object parameters = null,
            bool external = false)
        {
            Id = id;
            RouteInfo = routeInfo;
            RouteValues = routeValues;
            Parameters = parameters;
            External = external;
        }
    }
}