using System.Linq;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Next.Web.Binders;

namespace Microsoft.AspNetCore.Mvc
{
    public static class MvcOptionsExtensions
    {
        public static MvcOptions AddBodyAndRouteBinding(this MvcOptions options)
        {
            var providers = options.ModelBinderProviders;
            var bodyProvider = providers.Single(provider => provider.GetType() == typeof(BodyModelBinderProvider)) as BodyModelBinderProvider;
            var complexProvider = providers.Single(provider => provider.GetType() == typeof(ComplexObjectModelBinderProvider)) as ComplexObjectModelBinderProvider;

            var bodyAndRouteProvider = new BodyAndRouteModelBinderProvider(bodyProvider, complexProvider);
            providers.Insert(0, bodyAndRouteProvider);

            return options;
        }
    }
}