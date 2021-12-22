using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

namespace Next.Web.Binders
{
    public class BodyAndRouteModelBinderProvider : IModelBinderProvider
    {
        private readonly BodyModelBinderProvider _bodyModelBinderProvider;
        private readonly ComplexObjectModelBinderProvider _complexTypeModelBinderProvider;

        public BodyAndRouteModelBinderProvider(
            BodyModelBinderProvider bodyModelBinderProvider,
            ComplexObjectModelBinderProvider complexTypeModelBinderProvider)
        {
            _bodyModelBinderProvider = bodyModelBinderProvider;
            _complexTypeModelBinderProvider = complexTypeModelBinderProvider;
        }

        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            var bodyBinder = _bodyModelBinderProvider.GetBinder(context);
            var complexBinder = _complexTypeModelBinderProvider.GetBinder(context);

            if (context.BindingInfo.BindingSource != null &&
                context.BindingInfo.BindingSource.CanAcceptDataFrom(BodyAndRouteBindingSource.BodyAndRoute))
            {
                return new BodyAndRouteModelBinder(bodyBinder, complexBinder);
            }

            return null;
        }
    }
}