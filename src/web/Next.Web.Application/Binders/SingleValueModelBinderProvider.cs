using System;
using System.Collections.Concurrent;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Next.Abstractions.Domain.ValueObjects;

namespace Next.Web.Application.Binders
{
    internal class SingleValueModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            
            return typeof(ISingleValueObject).IsAssignableFrom(context.Metadata.ModelType) ? 
                new BinderTypeModelBinder(typeof(SingleValueBinder)) : 
                null;
        }
    }
}