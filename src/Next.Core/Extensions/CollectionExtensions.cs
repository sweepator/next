namespace System.Collections.Generic
{
    /// <summary>
    /// Provides extension methods for collections
    /// </summary>
    public static class CollectionExtensions
    {
        public static TValue GetOrAdd<TKey, TValue>(
            this IDictionary<TKey, TValue> source, 
            TKey key, 
            Func<TKey, TValue> createValue)
        {
            if (!source.TryGetValue(key, out var value))
            {
                source[key] = value = createValue(key);
            }

            return value;
        }

        public static TValue GetOrDefault<TKey, TValue>(
            this IReadOnlyDictionary<TKey, TValue> source, 
            TKey key)
        {
            return source.TryGetValue(key, out var value) ? value : default;
        }
    }
}