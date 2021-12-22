using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Next.Web.Hypermedia
{
    public class LinkedResourceList : LinkedResource, ILinkedResourceList
    {
        private readonly IEnumerable _resource;
        
        public IEnumerable<ContentLinkedResource> Content { get; }
        
        public LinkedResourceList(IEnumerable resource)
        {
            _resource = resource ?? throw new ArgumentNullException(nameof(resource));

            Content = (from object item in resource select new ContentLinkedResource(item))
                .ToList();
        }

        public override object GetResource()
        {
            return _resource;
        }
    }
}