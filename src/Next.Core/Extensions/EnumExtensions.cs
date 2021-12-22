using Next.Core.Attributes;
using System.Reflection;
using System.Linq;
using System.Collections.Concurrent;

namespace System
{
    public static class EnumExtensions
    {
        private static ConcurrentDictionary<Enum, Guid> EnumGuidMapping = new ConcurrentDictionary<Enum, Guid>();
        private static ConcurrentDictionary<Guid, Enum> EnumMapping = new ConcurrentDictionary<Guid, Enum>();

        public static Guid GetEnumGuid(this Enum e)
        {
            return EnumGuidMapping.GetOrAdd(e, k => 
            {
                Type type = e.GetType();

                MemberInfo[] memInfo = type.GetMember(e.ToString());

                if (memInfo != null && memInfo.Length > 0)
                {
                    object[] attrs = memInfo[0].GetCustomAttributes(typeof(EnumGuid), false);
                    if (attrs != null && attrs.Length > 0)
                    {
                        return ((EnumGuid)attrs[0]).Id;
                    }
                }

                throw new ArgumentException("Enum " + e.ToString() + " has no EnumGuid defined!");
            });
        }

        public static T GetEnum<T>(this Guid guid)
            where T : Enum
        {
            return (T)EnumMapping.GetOrAdd(guid,
                k =>
                {
                    return Enum.GetValues(typeof(T))
                        .Cast<T>()
                        .FirstOrDefault(o => o.GetEnumGuid() == k);
                });
        }
    }
}
