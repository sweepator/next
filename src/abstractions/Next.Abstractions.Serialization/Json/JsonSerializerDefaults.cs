using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Next.Abstractions.Serialization.Json
{
    public static class JsonSerializerDefaults
    {
        public static JsonSerializerOptions GetDefaultSettings(IEnumerable<JsonConverter> jsonConverters = null)
        {
            var settings = new JsonSerializerOptions();
            settings.AddDefaults();
            
            if (jsonConverters != null)
            {
                foreach (var jsonConverter in jsonConverters)
                {
                    settings.Converters.Add(jsonConverter);
                }
            }

            return settings;
        }

        public static JsonSerializerOptions AddDefaults(this JsonSerializerOptions settings)
        {
            settings.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            settings.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            settings.Converters.Add( new JsonStringEnumConverter());
            
            return settings;
        }
    }
}