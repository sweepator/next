using System.Collections.Generic;
using System.Reflection;

namespace Next.Web.Hypermedia
{
    public interface IHateoasAssemblyLoader
    {
        IEnumerable<Assembly> GetAssemblies();
    }
}