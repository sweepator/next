using System;
using System.Collections.Generic;

namespace Next.Core.Metadata
{
    public interface IMetadataContainer : IDictionary<string, string>
    {
        string GetMetadataValue(string key);
        T GetMetadataValue<T>(string key, Func<string, T> converter);
    }
}