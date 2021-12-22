using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;

namespace Next.Abstractions.Domain
{
    public class DomainEventFactory : IDomainEventFactory
    {
        private static readonly ConcurrentDictionary<Type, Type> AggregateEventToDomainEventTypeMap = new();
        private static readonly ConcurrentDictionary<Type, Type> DomainEventToIdentityTypeMap = new();

        public IDomainEvent Create(
            IAggregateEvent aggregateEvent,
            IMetadata metadata,
            string aggregateIdentity,
            int version,
            DateTime timeStamp)
        {
            var domainEventType = AggregateEventToDomainEventTypeMap.GetOrAdd(aggregateEvent.GetType(), GetDomainEventType);
            var identityType = DomainEventToIdentityTypeMap.GetOrAdd(domainEventType, GetIdentityType);
            var identity = Activator.CreateInstance(identityType, aggregateIdentity);

            var domainEvent = (IDomainEvent)Activator.CreateInstance(
                domainEventType,
                aggregateEvent,
                metadata,
                timeStamp,
                identity,
                version);

            return domainEvent;
        }

        public IDomainEvent<TAggregate, TIdentity> Create<TAggregate, TIdentity>(
            IAggregateEvent aggregateEvent,
            IMetadata metadata,
            TIdentity id,
            int version,
            DateTime timeStamp)
            where TAggregate : IAggregateRoot<TIdentity>
            where TIdentity : IIdentity
        {
            return (IDomainEvent<TAggregate, TIdentity>)Create(
                aggregateEvent,
                metadata,
                id.Value,
                version,
                timeStamp);
        }

        public IDomainEvent<TAggregate, TIdentity> Upgrade<TAggregate, TIdentity>(
            IDomainEvent domainEvent,
            IAggregateEvent aggregateEvent)
            where TAggregate : IAggregateRoot<TIdentity>
            where TIdentity : IIdentity
        {
            return Create<TAggregate, TIdentity>(
                aggregateEvent,
                domainEvent.Metadata,
                (TIdentity) domainEvent.AggregateIdentity,
                domainEvent.Version,
                domainEvent.Timestamp);
        }

        public Type GetIdentityType(Type domainEventType)
        {
            var domainEventInterfaceType = domainEventType
                .GetTypeInfo()
                .GetInterfaces()
                .SingleOrDefault(i => IntrospectionExtensions.GetTypeInfo(i).IsGenericType && i.GetGenericTypeDefinition() == typeof(IDomainEvent<,>));

            if (domainEventInterfaceType == null)
            {
                throw new ArgumentException($"Type '{domainEventType.Name}' is not a '{typeof(IDomainEvent<,>).Name}'");
            }

            var genericArguments = domainEventInterfaceType.GetTypeInfo().GetGenericArguments();
            return genericArguments[1];
        }

        public Type GetDomainEventType(Type aggregateEventType)
        {
            var aggregateEventInterfaceType = aggregateEventType
                .GetTypeInfo()
                .GetInterfaces()
                .SingleOrDefault(i => i.GetTypeInfo().IsGenericType && i.GetGenericTypeDefinition() == typeof(IAggregateEvent<>));

            if (aggregateEventInterfaceType == null)
            {
                throw new ArgumentException($"Type '{aggregateEventType.Name}' is not a '{typeof(IAggregateEvent<>).Name}'");
            }

            var genericArguments = aggregateEventInterfaceType.GetTypeInfo().GetGenericArguments();
            var aggregateType = genericArguments[0];
            genericArguments = aggregateType.BaseType.GetGenericArguments();
            var identityType = genericArguments[1];
            
            return typeof(DomainEvent<,,>).MakeGenericType(
                aggregateType, 
                identityType, 
                aggregateEventType);
        }
    }
}