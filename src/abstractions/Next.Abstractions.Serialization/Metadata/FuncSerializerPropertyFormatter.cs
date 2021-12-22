using System;

namespace Next.Abstractions.Serialization.Metadata
{
    internal class FuncSerializerPropertyFormatter<T>: ISerializerPropertyFormatter<T>
        where T: class
    {
        private readonly Func<T, string> _func;

        public FuncSerializerPropertyFormatter(Func<T, string> func)
        {
            _func = func ?? throw new ArgumentNullException(nameof(func));
        }

        public string Format(T instance)
        {
            return _func(instance);
        }

        public string Format(object instance)
        {
            return this.Format((T) instance);
        }
    }
}