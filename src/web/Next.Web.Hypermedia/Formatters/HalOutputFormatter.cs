using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace Next.Web.Hypermedia.Formatters
{
    public class HalOutputFormatter: TextOutputFormatter
    {
        private readonly SystemTextJsonOutputFormatter _innerFormatter;

        public HalOutputFormatter(SystemTextJsonOutputFormatter innerFormatter)
        {
            _innerFormatter = innerFormatter;
            SupportedMediaTypes.Add("application/hal+json");
            foreach (var encoding in innerFormatter.SupportedEncodings)
            {
                SupportedEncodings.Add(encoding);
            }
        }

        public override async Task WriteResponseBodyAsync(
            OutputFormatterWriteContext context, 
            Encoding selectedEncoding)
        {
            var linksService = (ILinksService)context.HttpContext.RequestServices.GetService(typeof(ILinksService));

            if (linksService == null)
            {
                throw new ApplicationException("Invalid LinkService object");
            }

            var linkedResource = LinkedResourceFactory.CreateLinkedResource(context.Object);
            await linksService.AddLinks(linkedResource);

            var wrapperContext = new OutputFormatterWriteContext(
                context.HttpContext,
                context.WriterFactory,
                linkedResource.GetType(),
                linkedResource);
            
            await _innerFormatter.WriteResponseBodyAsync(
                wrapperContext,
                selectedEncoding);
        }
    }
}