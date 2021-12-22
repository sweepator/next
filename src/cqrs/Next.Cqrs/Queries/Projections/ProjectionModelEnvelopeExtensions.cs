namespace Next.Cqrs.Queries.Projections
{
    public static class ProjectionModelEnvelopeExtensions
    {
        public static ProjectionModelUpdateResult<TProjectionModel> AsUnmodifedResult<TProjectionModel>(
            this ProjectionModelEnvelope<TProjectionModel> readModelEnvelope)
            where TProjectionModel: class, IProjectionModel
        {
            return ProjectionModelUpdateResult<TProjectionModel>.With(
                readModelEnvelope,
                false);
        }

        public static ProjectionModelUpdateResult<TProjectionModel> AsModifedResult<TProjectionModel>(
            this ProjectionModelEnvelope<TProjectionModel> readModelEnvelope,
            TProjectionModel readModel,
            int? version = null)
            where TProjectionModel: class, IProjectionModel
        {
            return ProjectionModelUpdateResult<TProjectionModel>.With(
                ProjectionModelEnvelope<TProjectionModel>.With(readModelEnvelope.Id, readModel, version), 
                true);
        }
    }
}