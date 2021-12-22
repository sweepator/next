using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Logging;
using Next.Abstractions.Mapper.Exceptions;
using Next.Cqrs.Queries.Projections;
using Next.Cqrs.Queries.Services;

namespace Next.Cqrs.Queries.SqlServer.Services
{
    public class SqlServerQueryService<TQueryRequest, TProjectionModel> : IQueryService<TQueryRequest, TProjectionModel>
        where TQueryRequest : IQueryRequest<TProjectionModel>
        where TProjectionModel: class, IProjectionModel
    {
        private readonly ISqlServerQueryStore<TProjectionModel> _projectionModelStore;
        private readonly ILogger<SqlServerQueryService<TQueryRequest, TProjectionModel>> _logger;

        public SqlServerQueryService(
            ISqlServerQueryStore<TProjectionModel> projectionModelStore,
            ILogger<SqlServerQueryService<TQueryRequest, TProjectionModel>> logger)
        {
            _projectionModelStore = projectionModelStore;
            _logger = logger;
        }
        
        public async Task<IEnumerable<TProjectionModel>> Get(TQueryRequest queryRequest)
        {
            var queryPredicate = queryRequest.Map();

            if (queryPredicate == null)
            {
                _logger.LogError(
                    "Error mapping command SqlServerQueryPredicate from queryRequest {TQueryRequest}",
                    typeof(TQueryRequest).Name);
                throw new MappingException(
                    $"Error mapping command SqlServerQueryPredicate from queryRequest {typeof(TQueryRequest).Name}");
            }
            
            var dynamicParameters = new DynamicParameters();
            var where = new StringBuilder();

            foreach (var queryFilter in queryPredicate.Filters)
            {
                dynamicParameters.Add(queryFilter.Name, queryFilter.Value);
                if (where.Length > 0)
                {
                    where.Append(" AND ");
                }
                where.Append($"{queryFilter.Name} = @{queryFilter.Name}");
            }

            return await _projectionModelStore.Find(
                dynamicParameters,
                where.ToString());
        }
    }
}