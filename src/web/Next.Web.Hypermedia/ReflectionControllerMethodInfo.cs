using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Next.Web.Hypermedia
{
    public class ReflectionControllerMethodInfo : IControllerMethodInfo
    {
        private readonly MethodInfo _methodInfo;
        
        public Type ControllerType => _methodInfo.DeclaringType;

        public Type ReturnType => _methodInfo.ReturnType;

        public string MethodName => _methodInfo.Name;
        
        public ReflectionControllerMethodInfo(MethodInfo methodInfo)
        {
            _methodInfo = methodInfo;
        }

        public IEnumerable<TAttribute> GetAttributes<TAttribute>() where TAttribute : Attribute
        {
            return _methodInfo.GetCustomAttributes<TAttribute>()
                .Union(ControllerType.GetTypeInfo().GetCustomAttributes<TAttribute>());
        }
    }
}