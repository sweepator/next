using Next.Cqrs.Queries.Projections;

namespace Next.Cqrs.Queries.SqlServer
{
    public interface IProjectionModelSqlGenerator
    {
        string CreateInsertSql<TProjectionModel>()
            where TProjectionModel : IProjectionModel;

        string CreateSelectSql<TProjectionModel>()
            where TProjectionModel : IProjectionModel;

        string CreateSelectByIdentitySql<TProjectionModel>()
            where TProjectionModel : IProjectionModel;

        string CreateDeleteSql<TProjectionModel>()
            where TProjectionModel : IProjectionModel;

        string CreateUpdateSql<TProjectionModel>()
            where TProjectionModel : IProjectionModel;

        string CreatePurgeSql<TProjectionModel>()
            where TProjectionModel : IProjectionModel;
    }
}