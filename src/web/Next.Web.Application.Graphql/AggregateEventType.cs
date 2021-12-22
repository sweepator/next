using HotChocolate.Types;
using Next.Abstractions.Domain;

namespace Next.Web.Application.Graphql
{
    public class AggregateEventType<TAggregateEvent> : ObjectType<TAggregateEvent>
        where TAggregateEvent: IAggregateEvent
    {
        protected override void Configure(IObjectTypeDescriptor<TAggregateEvent> descriptor)
        {
            var domainEventName = typeof(TAggregateEvent).Name.ToLower();
                
            descriptor.BindFieldsImplicitly();
            descriptor.Name(domainEventName);
        }
    }
}