using Microsoft.AspNetCore.Http;

namespace Next.Web
{
    public abstract class HttpStrategy<T>: IHttpStrategy<T>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;      
    
        public HttpStrategy(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public virtual T GetHttpStrategyObject()
        {
            var httpContext = _httpContextAccessor.HttpContext; 

            if (httpContext == null)
            {
                return default(T);
            }

            return this.InternalGetStrategyValue(httpContext);
        }

        protected abstract T InternalGetStrategyValue(HttpContext httpContext);
    }
}
