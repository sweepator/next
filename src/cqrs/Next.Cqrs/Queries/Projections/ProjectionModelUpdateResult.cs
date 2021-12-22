namespace Next.Cqrs.Queries.Projections
{
    public abstract class ProjectionModelUpdateResult
    {
        public bool IsModified { get; }

        protected ProjectionModelUpdateResult(bool isModified)
        {
            IsModified = isModified;
        }
    }
    
    public class ProjectionModelUpdateResult<TProjectionModel> : ProjectionModelUpdateResult
        where TProjectionModel : class, IProjectionModel
    {
        public ProjectionModelEnvelope<TProjectionModel> Envelope { get; }

        private ProjectionModelUpdateResult(
            ProjectionModelEnvelope<TProjectionModel> envelope,
            bool isModified)
            : base(isModified)
        {
            Envelope = envelope;
        }

        public static ProjectionModelUpdateResult<TProjectionModel> With(
            ProjectionModelEnvelope<TProjectionModel> projectionModelEnvelope,
            bool isModified)
        {
            return new(
                projectionModelEnvelope,
                isModified);
        }

        public static ProjectionModelUpdateResult<TProjectionModel> With(
            object id,
            TProjectionModel projectionModel,
            int? version)
        {
            return new(
                ProjectionModelEnvelope<TProjectionModel>.With(
                    id, 
                    projectionModel, 
                    version),
                true);
        }
    }
}