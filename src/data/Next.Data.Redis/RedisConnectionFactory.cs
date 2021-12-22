using System.Collections.Concurrent;
using StackExchange.Redis;

namespace Next.Data.Redis
{
    public class RedisConnectionFactory: IRedisConnectionFactory
    {
        private static readonly ConcurrentDictionary<string, ConnectionMultiplexer> Connections = new();
        
        public ConnectionMultiplexer GetConnection(string connectionString)
        {
            return Connections.GetOrAdd(connectionString,
                connection =>
                {
                    var configurationOptions = new ConfigurationOptions();
                    configurationOptions.EndPoints.Add(connectionString);
                    return ConnectionMultiplexer.Connect(configurationOptions);
                });
        }
    }
}