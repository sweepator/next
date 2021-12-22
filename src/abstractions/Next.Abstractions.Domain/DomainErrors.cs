using Next.Core.Errors;

namespace Next.Abstractions.Domain
{
    public static class DomainErrors
    {
        public static readonly Error NotFound = new Error(nameof(NotFound),"Entity not found");
        public static readonly Error IsNew = new Error(nameof(IsNew),"Entity is new");
        public static readonly Error IsNotNew = new Error(nameof(IsNotNew),"Entity is not new");
    }
}