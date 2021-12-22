using System;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Next.Web.Binders
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Parameter, Inherited = true, AllowMultiple = false)]
    public class FromRouteAndQueryAttribute : Attribute, IBindingSourceMetadata
    {
        public BindingSource BindingSource { get; } = CompositeBindingSource.Create(
            new[] { BindingSource.Path, BindingSource.Query }, nameof(FromRouteAndQueryAttribute));
    }
}