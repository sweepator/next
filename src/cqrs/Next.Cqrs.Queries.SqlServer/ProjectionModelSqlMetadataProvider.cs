using System;

namespace Next.Cqrs.Queries.SqlServer
{
    public class ProjectionModelSqlMetadataProvider : IProjectionModelSqlMetadataProvider
    {
        public TableInfoMetadata GetTableInfo(Type projectionModelType)
        {
            return new(
                projectionModelType.Name,
                "dbo",
                "Id",
                "Version",
                Array.Empty<string>());
        }
    }
}