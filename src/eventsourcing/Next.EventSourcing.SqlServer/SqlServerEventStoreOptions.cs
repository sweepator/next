namespace Next.EventSourcing.SqlServer
{
    public class SqlServerEventStoreOptions
    {
        public string ConnectionString { get; set; }
        
        public double TimeoutSeconds { get; set; } = 30;

        public string SchemaName { get; set; } = Consts.DefaultSchemaName;

        public string EventTableName { get; set; } = Consts.DefaultEventTableName;
        
        public string SnapshotTableName { get; set; } = Consts.DefaultSnapShotTableName;
    }
}