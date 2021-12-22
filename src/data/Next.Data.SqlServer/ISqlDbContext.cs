using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace Next.Data.SqlServer
{
    public interface ISqlDbContext: IDisposable
    {
        SqlConnection Connection { get; }

        Task BeginTransaction(CancellationToken cancellationToken = default);
        
        Task BeginTransaction(
            IsolationLevel isolationLevel,
            CancellationToken cancellationToken = default);
        
        Task CommitTransaction(CancellationToken cancellationToken = default);

        public Task<int> ExecuteAsync(
            string sql,
            object param = null,
            int? commandTimeout = null,
            CommandType? commandType = null);

        Task<IEnumerable<T>> QueryAsync<T>(
            string sql,
            object param = null,
            int? commandTimeout = null,
            CommandType? commandType = null);
        
        Task<long> ExecuteScalar(
            string sql,
            object param = null,
            int? commandTimeout = null,
            CommandType? commandType = null);
    }
}