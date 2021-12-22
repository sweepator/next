using System;
using System.Collections.Concurrent;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace Next.Data.SqlServer
{
    public class SqlDbContextFactory: ISqlDbContextFactory
    {
        private readonly ConcurrentDictionary<string, SqlDbContext> _connections = new();
        
        public ISqlDbContext GetSqlDbContext(string connectionString)
        {
            var isNew = false;
            var sqlDbContext = _connections.GetOrAdd(connectionString,
                connection =>
                {
                    var instance =  new SqlDbContext(
                        this,
                        new SqlConnection(connection)); 
                    instance.Connection.Open();
                    isNew = true;
                    return instance;
                });

            if(!isNew)
            {
                sqlDbContext.DisposeCounter++;
            }
           
            return sqlDbContext;
        }

        internal void RemoveSqlDbContext(ISqlDbContext sqlDbContext)
        {
            var key = _connections
                .Single(o => o.Value == sqlDbContext)
                .Key;
            _connections.TryRemove(key, out _);
        }
    }
}