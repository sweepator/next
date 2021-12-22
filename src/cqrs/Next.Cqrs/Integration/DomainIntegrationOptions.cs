using System;
using Next.Abstractions.Domain;

namespace Next.Cqrs.Integration
{
    public class DomainIntegrationOptions<TIntegrationEvent>
        where TIntegrationEvent:class
    {
        public Func<IDomainEvent, TIntegrationEvent> MapFunc { get; set; } = _ => null;
    }
}