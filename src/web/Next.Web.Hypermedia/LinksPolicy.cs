using System;
using System.Collections.Generic;

namespace Next.Web.Hypermedia
{
    public class LinksPolicy : ILinksPolicy
    {
        public IReadOnlyList<ILinksRequirement> Requirements { get; }
        
        public LinksPolicy(IEnumerable<ILinksRequirement> requirements)
        {
            if (requirements == null)
            {
                throw new ArgumentNullException(nameof(requirements));
            }

            Requirements = new List<ILinksRequirement>(requirements).AsReadOnly();
        }
    }
}