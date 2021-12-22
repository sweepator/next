using System;

namespace Next.Abstractions.Data
{
    public class DataMigration
    {
        public string MigrationName { get; }
        public DateTime Timestamp { get; }

        public DataMigration(
            string migrationName, 
            DateTime timestamp)
        {
            MigrationName = migrationName;
            Timestamp = timestamp;
        }
    }
}