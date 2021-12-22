using System.Collections.Generic;

namespace Next.Cqrs.Queries.SqlServer
{
    public class TableInfoMetadata
    {
        public string Name { get; }
        public string Schema { get; }
        public string IdentityColumnName { get; }
        public string VersionColumnName { get; }
        public IEnumerable<string> IgnoredColumns { get; }
        
        public TableInfoMetadata(
            string name, 
            string schema, 
            string identityColumnName, 
            string versionColumnName,
            IEnumerable<string> ignoredColumns)
        {
            Name = name;
            Schema = schema;
            IdentityColumnName = identityColumnName;
            VersionColumnName = versionColumnName;
            IgnoredColumns = ignoredColumns;
        }
    }
}