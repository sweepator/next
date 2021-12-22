using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using Next.Abstractions.Domain;
using Next.Core.Reflection;

namespace Next.Cqrs.Queries.Projections
{
    public class ProjectionModelDomainEventApplier : IProjectionModelDomainEventApplier
    { 
        private const string ApplyMethodName = "Apply";

        private static readonly ConcurrentDictionary<Type, ConcurrentDictionary<Type, ApplyMethod>> ApplyMethods = new ConcurrentDictionary<Type, ConcurrentDictionary<Type, ApplyMethod>>();

        public bool UpdateProjectionModel<TReadModel>(
            TReadModel projectionModel,
            IEnumerable<IDomainEvent> domainEvents,
            IProjectionModelContext projectionModelContext)
            where TReadModel : IProjectionModel
        {
            var readModelType = typeof(TReadModel);
            var appliedAny = false;

            foreach (var domainEvent in domainEvents)
            {
                var applyMethods = ApplyMethods.GetOrAdd(
                    readModelType,
                    t => new ConcurrentDictionary<Type, ApplyMethod>());
                
                var applyMethod = applyMethods.GetOrAdd(
                    domainEvent.EventType,
                    t =>
                    {
                        var domainEventType = typeof(IDomainEvent<,,>).MakeGenericType(
                            domainEvent.AggregateType,
                            domainEvent.AggregateIdentity.GetType(), 
                            t);

                        var methodSignature = new[]
                        {
                            typeof(IProjectionModelContext), 
                            domainEventType
                        };
                        var methodInfo = readModelType.GetTypeInfo().GetMethod(
                            ApplyMethodName, 
                            methodSignature);

                        if (methodInfo != null)
                        {
                            var method = ReflectionHelper
                                .CompileMethodInvocation<Action<IProjectionModel, IProjectionModelContext, IDomainEvent>>(methodInfo);
                            return new ApplyMethod(method);
                        }
                        
                        return null;
                    });

                if (applyMethod != null)
                {
                    applyMethod.Apply(projectionModel, projectionModelContext, domainEvent);
                    appliedAny = true;
                }
            }

            return appliedAny;
        }

        private class ApplyMethod
        {
            private readonly Action<IProjectionModel, IProjectionModelContext, IDomainEvent> _syncMethod;

            public ApplyMethod(Action<IProjectionModel, IProjectionModelContext, IDomainEvent> syncMethod)
            {
                _syncMethod = syncMethod;
            }

            public void Apply(
                IProjectionModel readModel, 
                IProjectionModelContext context, 
                IDomainEvent domainEvent)
            {
                _syncMethod(readModel, context, domainEvent);
                readModel.Version = domainEvent.Version;
            }
        }
    }
}