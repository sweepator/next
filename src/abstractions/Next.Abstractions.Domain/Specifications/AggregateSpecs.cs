using Next.Core.Specifications;

namespace Next.Abstractions.Domain.Specifications
{
    public static class AggregateSpecs
    {
        public static ISpecification<IAggregateRoot> AggregateIsNew { get; } = new AggregateIsNewSpecification();
        public static ISpecification<IAggregateRoot> AggregateIsCreated { get; } = new AggregateIsCreatedSpecification();
    }
}