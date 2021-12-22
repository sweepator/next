using System;
using System.Net.Http;

namespace Next.Web.Hypermedia
{
    public class RouteInfo
    {
        public string RouteName { get; }
        
        public HttpMethod HttpMethod { get; }
        
        public IControllerMethodInfo MethodInfo { get; }
        
        public Type ControllerType => MethodInfo?.ControllerType;
        
        public string MethodName => MethodInfo?.MethodName;
        
        public Type ReturnType => MethodInfo.ReturnType;
        
        public string ControllerName => ControllerType?.Name?.Replace(
            "Controller", 
            string.Empty);
        
        public RouteInfo(
            string name, 
            HttpMethod httpMethod, 
            IControllerMethodInfo methodInfo)
        {
            RouteName = name ?? $"{methodInfo?.ControllerType.Namespace}.{methodInfo?.ControllerType?.Name}.{methodInfo?.MethodName}";
            HttpMethod = httpMethod;
            MethodInfo = methodInfo;
        }
    }
}