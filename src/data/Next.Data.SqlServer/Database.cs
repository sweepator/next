using Microsoft.Data.SqlClient;

namespace Next.Data.SqlServer
{
    public static class Database
    {
        public static void EnsureDatabase(string dbConnectionString)
        {
            var builder = new SqlConnectionStringBuilder(dbConnectionString);
            var catalog = builder.InitialCatalog;
            builder.InitialCatalog = "master";

            using (var serverConnection = new SqlConnection(builder.ConnectionString))
            {
                serverConnection.Open();

                var databasesQuery = "SELECT * FROM sys.databases WHERE NAME = @name";
                var createDatabaseQuery = @"CREATE DATABASE [{0}]";

                using (var sqlCommand = new SqlCommand(databasesQuery, serverConnection))
                {
                    sqlCommand.Parameters.Add(new SqlParameter("name", catalog));
                    using (var dataReader = sqlCommand.ExecuteReader())
                    {
                        if (dataReader.HasRows)
                        {
                            return;
                        }
                    }
                }

                var createDatabaseCommand = string.Format(createDatabaseQuery, catalog);
                builder.InitialCatalog = catalog;

                using (var sqlCommand = new SqlCommand(createDatabaseCommand, serverConnection))
                {
                    sqlCommand.ExecuteNonQuery();
                }

                SqlConnection.ClearAllPools();
            }
        }
    }
}
