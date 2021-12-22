namespace Next.Data.DbUp.SqlServer
{
    public static class SqlServerDataMigrationsOptionsExtensions
    {
        public static string GetSchema(this SqlServerDataMigrationsOptions options)
        {
            return options.Variables.ContainsKey("SchemaName") ? options.Variables["SchemaName"] : "dbo";
        }
        
        public static void SetSchema(
            this SqlServerDataMigrationsOptions options,
            string schema)
        {
            options.Variables["SchemaName"] = schema;
        }
        
        public static string GetMigrationsTable(this SqlServerDataMigrationsOptions options)
        {
            return options.Variables.ContainsKey("MigrationsTable") ? options.Variables["MigrationsTable"] : "DataMigrations";
        }
        
        public static void SetMigrationsTable(
            this SqlServerDataMigrationsOptions options,
            string tableName)
        {
            options.Variables["MigrationsTable"] = tableName;
        }
    }
}