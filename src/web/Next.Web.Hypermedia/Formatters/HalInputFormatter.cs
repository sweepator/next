using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace Next.Web.Hypermedia.Formatters
{
    public class HalInputFormatter : TextInputFormatter
    {
        private readonly SystemTextJsonInputFormatter _innerFormatter;

        public HalInputFormatter(SystemTextJsonInputFormatter innerFormatter)
        {
            _innerFormatter = innerFormatter;
            SupportedMediaTypes.Add("application/hal+json");
            foreach (var encoding in innerFormatter.SupportedEncodings)
            {
                SupportedEncodings.Add(encoding);
            }
        }
        
        public override async Task<InputFormatterResult> ReadRequestBodyAsync(
            InputFormatterContext context, 
            Encoding encoding)
        {
            return await _innerFormatter.ReadRequestBodyAsync(
                context,
                encoding);
        }
    }
}