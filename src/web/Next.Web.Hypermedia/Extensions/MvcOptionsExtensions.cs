using System.Linq;
using Microsoft.AspNetCore.Mvc.Formatters;
using Next.Web.Hypermedia.Formatters;

namespace Microsoft.AspNetCore.Mvc
{
    public static class MvcOptionsExtensions
    {
        public static MvcOptions AddHypermediaFormatters(this MvcOptions options)
        {
            var jsonOutputFormatter = options
                .OutputFormatters
                .OfType<SystemTextJsonOutputFormatter>()
                .FirstOrDefault();

            if (jsonOutputFormatter != null)
            {
                options.OutputFormatters.Add(new HalOutputFormatter(jsonOutputFormatter));
            }
            
            var jsonInputFormatter = options
                .InputFormatters
                .OfType<SystemTextJsonInputFormatter>()
                .FirstOrDefault();

            if (jsonInputFormatter != null)
            {
                options.InputFormatters.Add(new HalInputFormatter(jsonInputFormatter));
            }

            return options;
        }
    }
}