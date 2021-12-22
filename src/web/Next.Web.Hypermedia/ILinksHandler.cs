using System.Threading.Tasks;

namespace Next.Web.Hypermedia
{
    public interface ILinksHandler
    {
        Task HandleAsync(HateoasHandlerContext context);
    }
}