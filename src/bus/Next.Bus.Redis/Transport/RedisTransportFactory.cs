using Next.Abstractions.Bus.Transport;
using Next.Abstractions.Serialization.Json;
using Next.Data.Redis;
using StackExchange.Redis;

namespace Next.Bus.Redis.Transport
{
    public class RedisTransportFactory : BrokerlessTransportFactory
    {
        private readonly IRedisConnectionFactory _redisConnectionFactory;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly string _connectionString;
        private readonly string _prefix;

        public RedisTransportFactory(
            IRedisConnectionFactory redisConnectionFactory,
            IJsonSerializer jsonSerializer,
            string connectionString, 
            string prefix = null)
        {
            _redisConnectionFactory = redisConnectionFactory;
            _jsonSerializer = jsonSerializer;
            _connectionString = connectionString;
            _prefix = prefix;
        }

        protected override IInboundTransport CreateInboundTransport(string endpoint)
        {
            return new RedisTransport(
                _redisConnectionFactory,
                _jsonSerializer,
                _connectionString, 
                _prefix, 
                endpoint);
        }

        protected override IOutboundTransport CreateOutboundTransport()
        {
            return new RedisTransport(
                _redisConnectionFactory,
                _jsonSerializer,
                _connectionString, 
                _prefix, 
                null);
        }
    }
}