using System.Threading;
using System.Threading.Tasks;

namespace Next.Data.SqlServer
{
    public sealed class SqlDbContextSession : ISqlDbContextSession
    {
        private static readonly AsyncLocal<ContextHolder> Current = new();

        public SqlDbContextSession()
        {
            Context ??= new SqlDbContextFactory();
        }

        private ISqlDbContextFactory Context
        {
            get => Current.Value?.Context;
            set
            {
                var holder = Current.Value;
                if (holder != null)
                {
                    holder.Context = null;
                }

                if (value != null)
                {
                    Current.Value = new ContextHolder { Context = value };
                }
            }
        }

        public ISqlDbContext GetSqlDbContext(string connectionString)
        {
            return Context.GetSqlDbContext(connectionString);
        }

        private class ContextHolder
        {
            public ISqlDbContextFactory Context;
        }
    }
}