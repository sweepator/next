using System;

namespace Next.Core.Attributes
{
    public class EnumGuid : Attribute
    {
        public Guid Id { get; }

        public EnumGuid(string id)
        {
            Id = new Guid(id);
        }
    }
}
