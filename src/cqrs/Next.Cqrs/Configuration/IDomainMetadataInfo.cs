using System;
using System.Collections.Generic;

namespace Next.Cqrs.Configuration
{
    public interface IDomainMetadataInfo
    {
        IEnumerable<Type> AggregateRootTypes { get; }
        IEnumerable<Type> AggregateEventTypes { get; }
        IEnumerable<Type> SnapshotStateTypes { get; }
        IEnumerable<Type> ProjectionModelTypes { get; }
        IEnumerable<Type> CommandTypes { get; }
        IDictionary<Type, IEnumerable<Type>> AggregateEventsByAggregateRootType { get; }
        IDictionary<Type, IEnumerable<Type>> AggregateCommandsByAggregateRootType { get; }
        IDictionary<Type, IEnumerable<Type>> QueryRequestByProjectionTypes { get; }
    }
}