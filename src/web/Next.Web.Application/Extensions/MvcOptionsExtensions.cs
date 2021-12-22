using Next.Web.Application.Binders;

namespace Microsoft.AspNetCore.Mvc
{
    public static class MvcOptionsExtensions
    {
        public static MvcOptions AddSingleValueBinders(this MvcOptions options)
        {
            var providers = options.ModelBinderProviders;
            providers.Insert(0, new SingleValueModelBinderProvider());
            return options;
        }
    }
}