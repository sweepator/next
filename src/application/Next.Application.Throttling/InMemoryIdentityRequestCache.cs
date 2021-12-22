using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;

namespace Next.Application.Throttling
{
    public class InMemoryIdentityRequestCache : IIdentityRequestCache
    {
        private readonly IDistributedCache _distributedCache;

        public InMemoryIdentityRequestCache(IDistributedCache distributedCache)
        {
            _distributedCache = distributedCache;
        }
        
        public async Task<bool> SetId(Guid id, TimeSpan expiration)
        {
            var cacheData = await _distributedCache.GetAsync(id.ToString());

            if (cacheData != null)
            {
                return false;
            }

            var cacheEntryOptions = new DistributedCacheEntryOptions()
                .SetAbsoluteExpiration(expiration);
            cacheData = id.ToByteArray();
            await _distributedCache.SetAsync(id.ToString(), cacheData, cacheEntryOptions);
            
            return true;
        }
    }
}