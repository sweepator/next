namespace Next.Web.Hypermedia
{
    public interface IHateoasRouteMap
    {
        RouteInfo GetCurrentRoute();
        RouteInfo GetRoute(string name);
    }
}