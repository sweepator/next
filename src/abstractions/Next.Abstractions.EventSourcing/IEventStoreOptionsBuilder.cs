using System;
using Microsoft.Extensions.DependencyInjection;
using Next.Abstractions.EventSourcing.Metadata;
using Next.Abstractions.EventSourcing.Snapshot;

namespace Next.Abstractions.EventSourcing
{
    public interface IEventStoreOptionsBuilder
    {
        IServiceCollection Services { get; }

        IEventStoreOptionsBuilder AddMetadataProvider<TMetadataProvider>()
            where TMetadataProvider : class, IMetadataEnricher;
        
        IEventStoreOptionsBuilder AddSnapshotStrategy<TSnapshotStrategy>()
            where TSnapshotStrategy : class, ISnapshotStrategy;
        
        IEventStoreOptionsBuilder ConfigurePublisher(Action<EventPublisherOptions> setup);
    }
}
