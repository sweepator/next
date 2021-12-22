using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Next.Abstractions.Domain.Utils
{
    /// <summary>
    /// Helper class where most of the magic is done, which is the core of the simplicity of the Domain namespace.
    /// Contains helpers that generate runtime delegates using reflection and expression trees. These delegates should be cached by the caller since their execution is much faster than reflection invocations or expression trees compilation.
    /// </summary>
    internal class ReflectionUtils
    {
        public static Func<TIdentity, TState, TAggregate> BuildCreateAggregateFromStateFunc<TAggregate, TIdentity, TState>()
           where TAggregate : IAggregateRoot<TIdentity, TState>
           where TState : IState
           where TIdentity : IIdentity
        {
            var idParam = Expression.Parameter(typeof(TIdentity), "id");
            var stateParam = Expression.Parameter(typeof(TState), "state");

            var ctor = typeof(TAggregate).GetConstructor(new Type[] { typeof(TIdentity), typeof(TState) });

            var body = Expression.New(ctor, idParam, stateParam);
            var lambda = Expression.Lambda<Func<TIdentity, TState, TAggregate>>(body, idParam, stateParam);

            return lambda.Compile();
        }

        public static Dictionary<string, Action<TState, object>> GetStateEventMutators<TState>()
           where TState : IState
        {
            var stateType = typeof(TState);

            var mutateMethods = stateType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                                         .Where(m => m.Name.Length >= 2 && 
                                                m.Name.StartsWith("On") && 
                                                m.GetParameters().Length == 1 && 
                                                m.ReturnType == typeof(void))
                                         .ToArray();

            var stateEventMutators = from method in mutateMethods
                                     let eventType = method.GetParameters()[0].ParameterType
                                     select new
                                     {
                                         eventType.Name,
                                         Handler = BuildStateEventMutatorHandler<TState>(eventType, method)
                                     };

            return stateEventMutators.ToDictionary(m => m.Name, m => m.Handler);
        }

        private static Action<TState, object> BuildStateEventMutatorHandler<TState>(Type eventType, MethodInfo method)
            where TState : IState
        {
            var stateParam = Expression.Parameter(typeof(TState), "state");
            var eventParam = Expression.Parameter(typeof(object), "ev");

            // state.On((TEvent)ev)
            var methodCallExpr = Expression.Call(stateParam,
                                                 method,
                                                 Expression.Convert(eventParam, eventType));

            var lambda = Expression.Lambda<Action<TState, object>>(methodCallExpr, stateParam, eventParam);
            return lambda.Compile();
        }
    }
}
