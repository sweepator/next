using System;
using System.Collections.Generic;

namespace Next.Web.Hypermedia
{
    public interface IControllerMethodInfo
    {
        Type ControllerType { get; }
        Type ReturnType { get; }
        string MethodName { get; }
        
        IEnumerable<TAttribute> GetAttributes<TAttribute>() 
            where TAttribute: Attribute;
    }
}