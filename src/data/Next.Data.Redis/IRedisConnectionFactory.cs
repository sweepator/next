using StackExchange.Redis;

namespace Next.Data.Redis
{
    public interface IRedisConnectionFactory
    {
        ConnectionMultiplexer GetConnection(string connectionString);
    }
}