using System.Security.Principal;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddIdentityServices(this IServiceCollection services)
        {
            //Temporary
            return services.AddScoped<IPrincipal>(sp =>
            {
                var identity = new GenericIdentity("unknown");
                return new GenericPrincipal(identity, new string[] { });
            });

        }
    }
}
