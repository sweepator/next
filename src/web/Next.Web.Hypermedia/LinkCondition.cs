using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace Next.Web.Hypermedia
{
    public class LinkCondition<TResource>
    {
        public static readonly LinkCondition<TResource> None = new(
            false, 
            Enumerable.Empty<Func<TResource, HttpContext, bool>>(),
            Enumerable.Empty<string>());

        public IReadOnlyList<Func<TResource, HttpContext, bool>> Assertions { get; }
        public IReadOnlyList<string> AuthorizationPolicyNames { get; }
        public bool RequiresRouteAuthorization { get; set; }
        public bool RequiresAuthorization => RequiresRouteAuthorization || AuthorizationPolicyNames.Any();
        
        public LinkCondition(bool requiresRouteAuthorization, 
            IEnumerable<Func<TResource, HttpContext, bool>> assertions,
            IEnumerable<string> policyNames)
        {
            RequiresRouteAuthorization = requiresRouteAuthorization;
            Assertions = new List<Func<TResource, HttpContext, bool>>(assertions).AsReadOnly();
            AuthorizationPolicyNames = new List<string>(policyNames).AsReadOnly();
        }
    }
}