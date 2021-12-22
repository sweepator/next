using System;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Next.Web.Binders
{
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class FromBodyAndRouteAttribute :  Attribute, IBindingSourceMetadata
    {
        public BindingSource BindingSource => BodyAndRouteBindingSource.BodyAndRoute;
    }
}