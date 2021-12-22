using System;

namespace Next.Abstractions.Validation
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class ValidatorOrderAttribute: Attribute
    {
        public int Order { get; set; }
    }
}