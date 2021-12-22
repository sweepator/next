namespace Microsoft.Extensions.Hosting
{
    public static class HostEnvironmentExtensions
    {
        public static bool IsLocal(this IHostEnvironment hostEnvironment)
        {
            return hostEnvironment.IsEnvironment("Local");
        }
    }
}