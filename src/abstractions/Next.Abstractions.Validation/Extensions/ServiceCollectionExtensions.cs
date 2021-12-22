using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Next.Abstractions.Validation;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Add validator factory and register all provided validators
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddValidationFactory(this IServiceCollection services)
        {
            services.TryAddSingleton<IValidatorFactory>(s =>
            {
                var validatorFactory = new ValidatorFactory();
                var validators = s.GetServices<IValidator>();

                foreach (var val in validators)
                {
                    var validatorType = val.GetType()
                        .GetInterfaces()
                        .FirstOrDefault(o => o.GetGenericArguments().Any());

                    var order = val.GetType().GetCustomAttribute<ValidatorOrderAttribute>()?.Order;

                    if (validatorType != null && validatorType.GetGenericTypeDefinition() == typeof(IValidator<>))
                    {
                        var typeToValidate = validatorType.GetGenericArguments()[0];

                        if (!order.HasValue)
                        {
                            validatorFactory.RegisterValidator(typeToValidate, val);
                        }
                        else
                        {
                            validatorFactory.RegisterValidator(typeToValidate, val, order.Value);
                        }
                    }
                    else
                    {
                        if (!order.HasValue)
                        {
                            validatorFactory.RegisterValidator(typeof(object), val);
                        }
                        else
                        {
                            validatorFactory.RegisterValidator(typeof(object), val, order.Value);
                        }
                    }

                }

                return validatorFactory;
            });

            return services;
        }

        /// <summary>
        /// Add validators located on current assembly
        /// </summary>
        /// <param name="services"></param>
        /// <param name="assembly"></param>
        /// <returns></returns>
        public static IServiceCollection AddValidators(
            this IServiceCollection services, 
            Assembly assembly)
        {
            services.AddValidationFactory();
            
            var validatorTypes = assembly.GetTypes()
                .Where(t => !t.GetTypeInfo().IsAbstract &&
                            typeof(IValidator).GetTypeInfo().IsAssignableFrom(t))
                .ToList();

            foreach (var validatorType in validatorTypes)
            {
                services.Add(new ServiceDescriptor(typeof(IValidator), validatorType, ServiceLifetime.Singleton));
            }

            return services;
        }

        public static IServiceCollection AddValidator<TValidator>(this IServiceCollection services)
            where TValidator: class, IValidator
        {
            return services.AddSingleton<IValidator, TValidator>();
        }
    }
}