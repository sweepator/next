using Microsoft.AspNetCore.Mvc;
using Next.Web.Application.PortAdapters;

namespace Next.Web.Application.Controllers
{
    [ApiController]
    public abstract class BaseController: Controller
    {
        protected IHttpPortAdapter PortAdapter { get; }

        protected BaseController(IHttpPortAdapter httpPortAdapter)
        {
            PortAdapter = httpPortAdapter;
        }
    }
}