using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Next.Cqrs.Queries.Projections
{
    public class ProjectionModelFactory<TProjectionModel> : IProjectionModelFactory<TProjectionModel>
        where TProjectionModel : IProjectionModel
    {
        private readonly ILogger<ProjectionModelFactory<TProjectionModel>> _logger;

        static ProjectionModelFactory()
        {
            var type = typeof(TProjectionModel).GetTypeInfo();

            var emptyConstructor = type
                .GetConstructors()
                .Where(c => !c.GetParameters().Any())
                .ToList();

            if (!emptyConstructor.Any())
            {
                throw new ArgumentException($"Projection model type '{typeof(TProjectionModel).Name}' doesn't have an empty constructor. Please create a custom '{typeof(IProjectionModelFactory<TProjectionModel>).Name}' implementation.");
            }
        }

        public ProjectionModelFactory(ILogger<ProjectionModelFactory<TProjectionModel>> logger)
        {
            _logger = logger;
        }

        public Task<TProjectionModel> Create(
            object id,
            CancellationToken cancellationToken = default)
        {
            _logger.LogDebug($"Creating new instance of projection model type {typeof(TProjectionModel).Name} with ID {id}");

            var readModel = (TProjectionModel) Activator.CreateInstance(typeof(TProjectionModel));

            return Task.FromResult(readModel);
        }
    }
}