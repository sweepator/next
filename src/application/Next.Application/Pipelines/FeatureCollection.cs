using System;
using System.Collections;
using System.Collections.Generic;

namespace Next.Application.Pipelines
{
    public class FeatureCollection : IFeatureCollection
    {
        private IDictionary<Type, object> _features;

        private object this[Type key]
        {
            get
            {
                if (key == null)
                {
                    throw new ArgumentNullException(nameof(key));
                }

                return _features != null && _features.TryGetValue(key, out var result) ? result : null;
            }
            set
            {
                if (key == null)
                {
                    throw new ArgumentNullException(nameof(key));
                }

                if (value == null)
                {
                    _features?.Remove(key);
                    return;
                }

                _features ??= new Dictionary<Type, object>();
                _features[key] = value;
            }
        }

        public TFeature Get<TFeature>()
        {
            return (TFeature)this[typeof(TFeature)];
        }
        
        public void Set<TFeature>(TFeature feature)
        {
            this[typeof(TFeature)] = feature;
        }

        public IEnumerator<KeyValuePair<Type, object>> GetEnumerator()
        {
            return _features.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}