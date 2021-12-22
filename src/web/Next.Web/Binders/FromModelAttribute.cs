using System;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Next.Web.Binders
{
    public sealed class FromModelAttribute : Attribute, IBindingSourceMetadata
    {
        public BindingSource BindingSource { get; } = BindingSource.ModelBinding;
    }
}