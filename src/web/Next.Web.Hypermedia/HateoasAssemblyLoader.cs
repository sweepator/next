using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Next.Web.Hypermedia
{
    internal class HateoasAssemblyLoader: IHateoasAssemblyLoader
    {
        private readonly IEnumerable<Assembly> _assemblies;

        public HateoasAssemblyLoader(Assembly assembly)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }
            
            var assemblies = assembly
                .GetReferencedAssemblies()
                .Select(Assembly.Load)
                .ToList();

            assemblies.Add(assembly);
            _assemblies = assemblies;
        }

        public IEnumerable<Assembly> GetAssemblies()
        {
            return _assemblies;
        }
    }
}