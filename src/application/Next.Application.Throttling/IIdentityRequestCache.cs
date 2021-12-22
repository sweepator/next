using System;
using System.Threading.Tasks;

namespace Next.Application.Throttling
{
    public interface IIdentityRequestCache
    {
        Task<bool> SetId(Guid id, TimeSpan expiration);
    }
}