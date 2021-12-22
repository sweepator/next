using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Next.Abstractions.Domain.ValueObjects;

namespace Next.Web.Application.Binders
{
    internal class SingleValueBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                throw new ArgumentNullException(nameof(bindingContext));
            }
            
            var result = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            var value = result.Values.FirstOrDefault();

            if (string.IsNullOrEmpty(value))
            {
                bindingContext.Result = ModelBindingResult.Success(bindingContext.Model);
                return Task.CompletedTask;
            }
            
            var typeToConvert = bindingContext.ModelType;

            try
            {
                var singleValueObject = (ISingleValueObject)Activator.CreateInstance(typeToConvert, value);
                bindingContext.Result = ModelBindingResult.Success(singleValueObject);
            }
            catch (TargetInvocationException e)
            {
                bindingContext.ModelState.AddModelError(
                    bindingContext.ModelName,
                    e.GetBaseException().Message);
                bindingContext.Result = ModelBindingResult.Failed();
            }
            
            return Task.CompletedTask;
        }
    }
}