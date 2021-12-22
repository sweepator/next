using System;

namespace Next.Cqrs.Queries.SqlServer
{
    public interface IProjectionModelSqlMetadataProvider
    {
        TableInfoMetadata GetTableInfo(Type projectionModelType);
    }
}