using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Next.Abstractions.Domain.Utils;

namespace Next.Abstractions.Domain
{
    internal static class StateMutator
    {
        private static readonly ConcurrentDictionary<Type, IStateMutator> _mutators;

        static StateMutator()
        {
            _mutators = new ConcurrentDictionary<Type, IStateMutator>();
        }

        public static IStateMutator For(Type stateType)
        {
            return _mutators.GetOrAdd(stateType, t => CreateMutator(stateType));
        }

        private static IStateMutator CreateMutator(Type stateType)
        {
            var mutatorType = typeof(StateMutator<>).MakeGenericType(stateType);
            var mutator = Activator.CreateInstance(mutatorType);

            return mutator as IStateMutator;
        }
    }

    internal class StateMutator<TState> : IStateMutator
        where TState : IState
    {
        private readonly Dictionary<string, Action<TState, object>> _eventMutators;

        public StateMutator()
        {
            _eventMutators = ReflectionUtils.GetStateEventMutators<TState>();
        }

        public void Mutate(IState state, IAggregateEvent @event)
        {
            var eventMutator = _eventMutators[@event.GetEventName()];
            eventMutator((TState)state, @event);
        }
    }
}
