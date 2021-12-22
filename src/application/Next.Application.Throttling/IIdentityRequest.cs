using System.Collections.Generic;

namespace  Next.Application.Throttling
{
    public interface IIdentityRequest
    {
        IEnumerable<byte[]> GeIdentityComponents();
    }
}