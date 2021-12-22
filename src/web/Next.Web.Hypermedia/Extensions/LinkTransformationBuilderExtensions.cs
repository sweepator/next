using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Routing;

namespace Next.Web.Hypermedia
{
    public static class LinkTransformationBuilderExtensions
    {
        public static LinkTransformationBuilder AddProtocol(
            this LinkTransformationBuilder builder, 
            string scheme = null)
        {
            return builder.Add(ctx => string.Concat(scheme ?? ctx.HttpContext.Request.Scheme, "://"));
        }

        public static LinkTransformationBuilder AddHost(this LinkTransformationBuilder builder)
        {
            return builder.Add(ctx => ctx.HttpContext.Request.Host.ToUriComponent());
        }

        public static LinkTransformationBuilder AddRoutePath(this LinkTransformationBuilder builder)
        {
            return builder.Add(ctx =>
            {
                if (string.IsNullOrEmpty(ctx.LinkSpec.RouteName))
                {
                    throw new InvalidOperationException($"Invalid route specified in link specification.");
                }

                var path = ctx.LinkGenerator.GetPathByAction(
                    ctx.HttpContext, 
                    ctx.LinkSpec.RouteInfo.MethodName,
                    ctx.LinkSpec.ControllerName,
                    ctx.LinkSpec.RouteValues);

                if (string.IsNullOrEmpty(path))
                {
                    throw new InvalidOperationException($"Invalid path when adding route '{ctx.LinkSpec.RouteName}'. RouteValues: {string.Join(",", ctx.ActionContext.RouteData.Values.Select(x => string.Concat(x.Key, "=", x.Value)))}");
                }

                return path;
            });
        }
        
        public static LinkTransformationBuilder AddVirtualPath(
            this LinkTransformationBuilder builder,
            string path)
        {
            return builder.AddVirtualPath(ctx => path);
        }
        
        public static LinkTransformationBuilder AddVirtualPath(
            this LinkTransformationBuilder builder,
            Func<LinkTransformationContext, string> getPath)
        {
            return builder.Add(ctx =>
            {
                var path = getPath(ctx);
                if (!path.StartsWith("/"))
                {
                    path = string.Concat("/", path);
                }
                return path;
            });
        }
        
        public static LinkTransformationBuilder AddQueryStringValues(
            this LinkTransformationBuilder builder, 
            IDictionary<string, string> values)
        {
            return builder.Add(ctx =>
            {
                var queryString = string.Join("&", values.Select(v => $"{v.Key}={v.Value?.ToString()}"));
                return string.Concat("?", queryString);
            });
        }
        
        public static LinkTransformationBuilder AddFragment(
            this LinkTransformationBuilder builder, 
            Func<LinkTransformationContext, string> getFragment)
        {
            return builder.Add(ctx => string.Concat("#", getFragment(ctx)));
        }

        private static RouteValueDictionary GetValuesDictionary(object values)
        {
            if (values is RouteValueDictionary routeValuesDictionary)
            {
                return routeValuesDictionary;
            }

            if (values is IDictionary<string, object> dictionaryValues)
            {
                routeValuesDictionary = new RouteValueDictionary();
                foreach (var (key, value) in dictionaryValues)
                {
                    routeValuesDictionary.Add(key, value);
                }

                return routeValuesDictionary;
            }

            return new RouteValueDictionary(values);
        }
    }
}