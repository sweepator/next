using Next.Abstractions.Mapper;
using Automapper = AutoMapper;
using Next.Mapper.AutoMapper;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMaping(
            this IServiceCollection services,
            params Automapper.Profile[] profiles)
        {
            services.AddSingleton<IMapper>(new Mapper(profiles));
            return services;
        }
    }
}