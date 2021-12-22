using System;

namespace Next.Cqrs.Queries.Projections
{
    public abstract class ProjectionModelEnvelope
    {
        public object Id { get; }
        public int? Version { get; }
        
        protected ProjectionModelEnvelope(
            object id,
            int? version)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Version = version;
        }
    }
    
    public class ProjectionModelEnvelope<TProjectionModel> : ProjectionModelEnvelope
        where TProjectionModel : class, IProjectionModel
    {
        public TProjectionModel ProjectionModel { get; }
        
        private ProjectionModelEnvelope(
            object id,
            TProjectionModel projectionModel,
            int? version)
            : base(id, version)
        {
            ProjectionModel = projectionModel;
        }

        public static ProjectionModelEnvelope<TProjectionModel> Empty(object id)
        {
            return new ProjectionModelEnvelope<TProjectionModel>(
                id, 
                null, 
                null);
        }

        public static ProjectionModelEnvelope<TProjectionModel> With(
            object id,
            TProjectionModel projectionModel)
        {
            return new ProjectionModelEnvelope<TProjectionModel>(
                id, 
                projectionModel, 
                null);
        }

        public static ProjectionModelEnvelope<TProjectionModel> With(
            object id,
            TProjectionModel projectionModel,
            int? version)
        {
            return new ProjectionModelEnvelope<TProjectionModel>(
                id, 
                projectionModel, 
                version);
        }

        public static ProjectionModelEnvelope<TProjectionModel> With(
            object id,
            TProjectionModel projectionModel,
            int version)
        {
            return new ProjectionModelEnvelope<TProjectionModel>(
                id, 
                projectionModel, 
                version);
        }
    }
}