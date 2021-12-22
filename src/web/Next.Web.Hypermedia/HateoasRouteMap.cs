using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;

namespace Next.Web.Hypermedia
{
    public class HateoasRouteMap : IHateoasRouteMap
    {
        private readonly IActionContextAccessor _contextAccessor;

        private IDictionary<string, RouteInfo> RouteMap { get; } = new Dictionary<string, RouteInfo>();
        
        public HateoasRouteMap(
            IActionContextAccessor contextAccessor,
            IHateoasAssemblyLoader assemblyLoader)
        {
            if (assemblyLoader == null)
            {
                throw new ArgumentNullException(nameof(assemblyLoader));
            }

            _contextAccessor = contextAccessor;

            var assemblies = assemblyLoader.GetAssemblies();

            foreach (var asm in assemblies)
            {
                var controllers = asm.GetTypes()
                    .Where(type => typeof(ControllerBase).IsAssignableFrom(type));

                var controllerMethods = controllers.SelectMany(c => c.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .Where(m => m.IsDefined(typeof(HttpMethodAttribute)))
                    .SelectMany(m => m.GetCustomAttributes<HttpMethodAttribute>(), (m, attr) => new
                    {
                        Controller = c,
                        Method = m,
                        HttpAttribute = attr
                    }));

                foreach (var attr in controllerMethods.Where(a => !string.IsNullOrWhiteSpace(a.HttpAttribute.Name)))
                {
                    var method = ParseMethod(attr.HttpAttribute.HttpMethods);
                    RouteMap[attr.HttpAttribute.Name] = new RouteInfo(
                        attr.HttpAttribute.Name, 
                        method,
                        new ReflectionControllerMethodInfo(attr.Method));
                }
            }
        }
                
        public RouteInfo GetRoute(string name)
        {
            if (!RouteMap.ContainsKey(name))
            {
                return null;
            }
            return RouteMap[name];
        }

        public RouteInfo GetCurrentRoute()
        {
            if (!(_contextAccessor?.ActionContext?.ActionDescriptor is ControllerActionDescriptor action))
            {
                throw new InvalidOperationException($"Invalid action descriptor in route map");
            }
            
            var attr = action.EndpointMetadata.OfType<HttpMethodAttribute>().FirstOrDefault();
            if (attr == null)
            {
                throw new InvalidOperationException($"Unable to get HttpMethodAttribute in route map");
            }
            
            var method = ParseMethod(attr.HttpMethods);
            return new RouteInfo(attr.Name, method, new ReflectionControllerMethodInfo(action.MethodInfo));
        }

        private HttpMethod ParseMethod(IEnumerable<string> methods)
        {
            var method = HttpMethod.Get;
            if (methods != null && methods.Any())
            {
                method = new HttpMethod(methods.First());
            }
            return method;
        }
    }
}