using System.Net.Http;
using Microsoft.AspNetCore.Routing;

namespace Next.Web.Hypermedia
{
    public interface ILinkSpec
    {
        string Id { get; }
        RouteInfo RouteInfo { get; }
        RouteValueDictionary RouteValues { get; }
        HttpMethod HttpMethod { get; }
        public object Parameters { get; }
        public bool External { get; }
        string RouteName { get; }
        string ControllerName { get; }
    }
}