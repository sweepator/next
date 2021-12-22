using Dapper;

namespace Next.Cqrs.Queries.SqlServer.Services
{
    public record SqlServerQueryPredicate(
        DynamicParameters DynamicParameters, 
        string Where);
}