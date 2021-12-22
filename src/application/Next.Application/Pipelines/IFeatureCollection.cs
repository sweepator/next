using System;
using System.Collections.Generic;

namespace Next.Application.Pipelines
{
    public interface IFeatureCollection : IEnumerable<KeyValuePair<Type, object>>
    {
        TFeature Get<TFeature>();
        void Set<TFeature>(TFeature feature);
    }
}