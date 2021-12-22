namespace Next.Cqrs.Queries.SqlServer
{
    public class ProjectionModelSqlGeneratorConfiguration
    {
        public string TableQuotedIdentifierPrefix { get; set; }

        public string TableQuotedIdentifierSuffix { get; set; }

        public string ColumnQuotedIdentifierPrefix { get; set; }

        public string ColumnQuotedIdentifierSuffix { get; set; }
        
        public ProjectionModelSqlGeneratorConfiguration()
        {
            TableQuotedIdentifierPrefix = "[";
            TableQuotedIdentifierSuffix = "]";
            ColumnQuotedIdentifierPrefix = "[";
            ColumnQuotedIdentifierSuffix = "]";
        }
    }
}