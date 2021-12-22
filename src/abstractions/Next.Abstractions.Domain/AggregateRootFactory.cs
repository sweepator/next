using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using Next.Abstractions.Domain.Utils;

namespace Next.Abstractions.Domain
{
    public static class AggregateFactory
    {
        private static readonly ConcurrentDictionary<Type, object> Factories = new ConcurrentDictionary<Type, object>();

        public static IAggregateFactory<TAggregate, TIdentity, TState> For<TAggregate, TIdentity, TState>()
            where TAggregate : IAggregateRoot<TIdentity, TState>
            where TState : class, IState, new()
            where TIdentity : IIdentity
        {
            var factory = Factories.GetOrAdd(typeof(TAggregate), t => CreateFactory<TAggregate, TIdentity, TState>());

            return factory as IAggregateFactory<TAggregate, TIdentity, TState>;
        }

        private static IAggregateFactory<TAggregate, TIdentity, TState> CreateFactory<TAggregate, TIdentity, TState>()
            where TAggregate : IAggregateRoot<TIdentity, TState>
            where TState : class, IState, new()
            where TIdentity : IIdentity
        {
            var stateType = typeof(TAggregate).BaseType?.GetGenericArguments()
                .Single(o => o.GetTypeInfo() == typeof(TState));

            var factoryType = typeof(AggregateFactory<,,>).MakeGenericType(typeof(TAggregate), typeof(TIdentity), stateType);

            return Activator.CreateInstance(factoryType) as IAggregateFactory<TAggregate, TIdentity, TState>;
        }
    }

    internal class AggregateFactory<TAggregate, TIdentity, TState> : IAggregateFactory<TAggregate, TIdentity, TState>
        where TAggregate : IAggregateRoot<TIdentity,TState>
        where TState : class, IState, new()
        where TIdentity : IIdentity
    {
        static readonly Func<TIdentity, TState, TAggregate> CreateAggregate;

        static AggregateFactory()
        {
            // runtime generated function that calls the aggregate's constructor passing the state as parameter
            CreateAggregate = ReflectionUtils.BuildCreateAggregateFromStateFunc<TAggregate, TIdentity, TState>();
        }

        public TAggregate CreateNew(TIdentity id)
        {
            var state = new TState();

            return CreateAggregate(id, state);
        }

        public TAggregate CreateFromEvents(
            TIdentity id, 
            IAggregateEvent[] events)
        {
            var state = new TState();

            // rebuild the state from past events
            foreach (var @event in events)
            {
                state.Mutate(@event);
            }

            return CreateAggregate(id, state);
        }
        
        public TAggregate Create(
            TIdentity id,
            TState state,
            IAggregateEvent[] events)
        {
            // rebuild the state from past events
            foreach (var @event in events)
            {
                state.Mutate(@event);
            }

            return CreateAggregate(id, state);
        }

        public TAggregate CreateFromState(
            TIdentity id, 
            TState state)
        {
            return CreateAggregate(id, state);
        }
    }
}
