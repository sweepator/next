using System;
using System.Collections.Generic;
using System.Linq;

namespace Next.Abstractions.Validation
{
    public sealed class ValidatorFactory : IValidatorFactory
    {
        private readonly List<Tuple<Type, Func<IValidator>>> _validators = new();

        public void RegisterValidator<T>(IValidator validator)
        {
            _validators.Add(new Tuple<Type, Func<IValidator>>(typeof(T), () => validator));
        }

        public void RegisterValidator<T>(
            IValidator validator, 
            int index)
        {
            _validators.Insert(index, new Tuple<Type, Func<IValidator>>(typeof(T), () => validator));
        }

        public void RegisterValidator(
            Type typeToValidate, 
            IValidator validator)
        {
            _validators.Add(new Tuple<Type, Func<IValidator>>(typeToValidate, () => validator));
        }

        public void RegisterValidator(
            Type typeToValidate, 
            IValidator validator, 
            int index)
        {
            _validators.Insert(index, new Tuple<Type, Func<IValidator>>(typeToValidate, () => validator));
        }

        public void RegisterValidator<T>(Func<IValidator> validatorBuilder)
        {
            _validators.Add(new Tuple<Type, Func<IValidator>>(typeof(T), validatorBuilder));
        }

        public void RegisterValidator<T>(
            Func<IValidator> validatorBuilder, 
            int index)
        {
            _validators.Insert(index, new Tuple<Type, Func<IValidator>>(typeof(T), validatorBuilder));
        }

        public IEnumerable<IValidator> GetValidators(object instance)
        {
            var instanceType = instance.GetType();
            return _validators.Select(v => v.Item2()).Where(v => v.CanValidate(instanceType)).ToArray();
        }
    }
}