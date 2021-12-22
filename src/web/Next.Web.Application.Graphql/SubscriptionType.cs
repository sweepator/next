using System.Linq;
using System.Reflection;
using HotChocolate;
using HotChocolate.Subscriptions;
using HotChocolate.Types;
using Next.Abstractions.Domain;
using Next.Cqrs.Configuration;

namespace Next.Web.Application.Graphql
{
    public class SubscriptionType: ObjectType
    {
        private readonly IDomainMetadataInfo _domainMetadataInfo;
            
        private static readonly MethodInfo AddDomainEventSubscriptionDescriptorMethod =
            typeof(SubscriptionType)
                .GetTypeInfo()
                .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                .Single(m => m.Name == nameof(AddAggregateEventSubscriptionDescriptor) && m.GetGenericArguments().Length == 1);

        public SubscriptionType(IDomainMetadataInfo domainMetadataInfo)
        {
            _domainMetadataInfo = domainMetadataInfo;
        }

        private void AddAggregateEventSubscriptionDescriptor<TAggregateEvent>(IObjectTypeDescriptor descriptor)
            where TAggregateEvent: IAggregateEvent
        {
            var eventName = typeof(TAggregateEvent).Name.ToLower();

            descriptor
                .Field(eventName)
                .Type<AggregateEventType<TAggregateEvent>>()
                .Resolve(context =>
                {
                    var eventMessage = context.GetEventMessage<TAggregateEvent>();
                    return eventMessage;
                })
                .Subscribe(async context =>
                {
                    var receiver = context.Service<ITopicEventReceiver>();
                    var stream = await receiver.SubscribeAsync<string, TAggregateEvent>(eventName);
                    return stream;
                });
        }
            
        protected override void Configure(IObjectTypeDescriptor descriptor)
        {
            descriptor.Name("subscription");
            
            foreach (var eventType in _domainMetadataInfo.AggregateEventTypes)
            {
                var method = AddDomainEventSubscriptionDescriptorMethod.MakeGenericMethod(eventType);
                method.Invoke(this, new object[] { descriptor });
            }
        }
    }
}