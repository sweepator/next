using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;

namespace Next.Data.SqlServer
{
    public class SqlDbContext : ISqlDbContext
    {
        private readonly SqlDbContextFactory _connectionFactory;
        public SqlConnection Connection { get; }
        
        private SqlTransaction Transaction { get; set; }
        
        internal int DisposeCounter { get; set; }
        private int TransactionCounter { get; set; }
        
        internal SqlDbContext(
            SqlDbContextFactory connectionFactory,
            SqlConnection connection)
        {
            _connectionFactory = connectionFactory;
            Connection = connection;
        }

        public async Task BeginTransaction(CancellationToken cancellationToken = default)
        {
            await BeginTransaction(
                IsolationLevel.Unspecified,
                cancellationToken);
        }

        public async Task BeginTransaction(
            IsolationLevel isolationLevel, 
            CancellationToken cancellationToken = default)
        {
            if (TransactionCounter == 0)
            {
                Transaction = (SqlTransaction) await Connection.BeginTransactionAsync(
                    isolationLevel,
                    cancellationToken);
            }

            TransactionCounter++;
        }

        public async Task CommitTransaction(CancellationToken cancellationToken = default)
        {
            TransactionCounter--;
            
            if(TransactionCounter == 0)
            {
                await Transaction.CommitAsync(cancellationToken);
                await Transaction.DisposeAsync();
                Transaction = null;
            }
        }
        
        public async Task<int> ExecuteAsync( 
            string sql, 
            object param = null,
            int? commandTimeout = null, 
            CommandType? commandType = null)
        {
            return await Connection.ExecuteAsync(
                sql,
                param,
                Transaction,
                commandTimeout,
                commandType);
        }

        public async Task<IEnumerable<T>> QueryAsync<T>(
            string sql, 
            object param = null,
            int? commandTimeout = null,
            CommandType? commandType = null)
        {
            return await Connection.QueryAsync<T>(
                sql,
                param,
                Transaction,
                commandTimeout,
                commandType);
        }

        public async Task<long> ExecuteScalar(
            string sql, 
            object param = null, 
            int? commandTimeout = null, 
            CommandType? commandType = null)
        {
            return await Connection.ExecuteScalarAsync<long>(
                sql,
                param,
                Transaction,
                commandTimeout,
                commandType);
        }

        public void Dispose()
        {
            if (DisposeCounter == 0)
            {
                _connectionFactory.RemoveSqlDbContext(this);

                if (Transaction != null)
                {
                    Transaction.Rollback();
                    Transaction.Dispose();
                    Transaction = null;
                }
                
                Connection.Dispose();
                return;
            }
            
            DisposeCounter--;
        }
    }
}