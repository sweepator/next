using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace Next.Web.Hypermedia
{
    public class LinkConditionBuilder<TResource>
    {
        private bool _requiresRouteAuthorization = false;
        private List<string> _authPolicyNames = new List<string>();
        private List<Func<TResource, HttpContext, bool>> _assertions = new List<Func<TResource, HttpContext, bool>>();
        
        public LinkConditionBuilder()
        {
        }

        public LinkConditionBuilder<TResource> AuthorizeRoute()
        {
            _requiresRouteAuthorization = true;
            return this;
        }
        
        public LinkConditionBuilder<TResource> Authorize(params string[] policyNames)
        {
            _authPolicyNames.AddRange(policyNames);
            return this;
        }

        public LinkConditionBuilder<TResource> Assert(Func<TResource, HttpContext, bool> condition)
        {
            _assertions.Add(condition);
            return this;
        }

        public LinkCondition<TResource> Build()
        {
            return new LinkCondition<TResource>(_requiresRouteAuthorization, _assertions, _authPolicyNames);
        }
    }
}