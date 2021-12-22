using System.Linq;
using System.Threading.Tasks;

namespace Next.Web.Hypermedia
{
    internal class PassThroughLinksHandler : ILinksHandler
    {
        public async Task HandleAsync(HateoasHandlerContext context)
        {
            foreach (var handler in context.Requirements.OfType<ILinksHandler>())
            {
                await handler.HandleAsync(context);
            }
        }
    }
}