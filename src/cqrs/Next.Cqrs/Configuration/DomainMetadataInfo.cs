using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Next.Abstractions.Domain;
using Next.Abstractions.EventSourcing;
using Next.Abstractions.EventSourcing.Snapshot;
using Next.Cqrs.Commands;
using Next.Cqrs.Queries;
using Next.Cqrs.Queries.Projections;

namespace Next.Cqrs.Configuration
{
    internal sealed class DomainMetadataInfo: IDomainMetadataInfo
    {
        public IEnumerable<Type> AggregateRootTypes => AggregateEventsByAggregateRootType.Keys;
        public IEnumerable<Type> AggregateEventTypes => AggregateEventsByAggregateRootType.Values.SelectMany(o => o);
        
        public IEnumerable<Type> SnapshotStateTypes { get; }
        public IEnumerable<Type> ProjectionModelTypes { get; }
        public IEnumerable<Type> CommandTypes { get; }
        
        public IDictionary<Type, IEnumerable<Type>> AggregateEventsByAggregateRootType { get; } = new Dictionary<Type, IEnumerable<Type>>();
        public IDictionary<Type, IEnumerable<Type>> AggregateCommandsByAggregateRootType { get; } = new Dictionary<Type, IEnumerable<Type>>();
        public IDictionary<Type, IEnumerable<Type>> QueryRequestByProjectionTypes { get; } = new Dictionary<Type, IEnumerable<Type>>();
        
        public DomainMetadataInfo(Assembly assembly)
        {
            if (assembly == null)
            {
                throw new ApplicationException(nameof(assembly));
            }
            
            var aggregateRootTypes = GetAggregateRootTypes(assembly);

            foreach (var aggregateRootType in aggregateRootTypes)
            {
                var aggregateEventByRootType = typeof(IAggregateEvent<>).MakeGenericType(aggregateRootType);
                    
                var aggregateEventTypes = assembly
                    .GetReferencedAssemblies()
                    .Select(Assembly.Load)
                    .Concat(new[] {assembly})
                    .SelectMany(a => a.GetTypes())
                    .Where(t => !t.GetTypeInfo().IsAbstract &&
                                aggregateEventByRootType.GetTypeInfo().IsAssignableFrom(t))
                    .ToList();

                AggregateEventsByAggregateRootType.Add(aggregateRootType, aggregateEventTypes);
            }

            ProjectionModelTypes = GetProjectionTypes(assembly);
            
            foreach (var projectionType in ProjectionModelTypes)
            {
                var queryRequestTypes = GetQueryRequestTypes(
                    assembly,
                    projectionType);
                QueryRequestByProjectionTypes.Add(projectionType, queryRequestTypes);
            }

            CommandTypes = GetCommands(assembly);

            foreach (var commandType in CommandTypes)
            {
                var aggregateRootType = commandType.BaseType?.GetGenericArguments()
                    .FirstOrDefault();

                if (aggregateRootType != null
                    && typeof(IAggregateRoot).IsAssignableFrom(aggregateRootType))
                {
                    if (!AggregateCommandsByAggregateRootType.ContainsKey(aggregateRootType))
                    {
                        AggregateCommandsByAggregateRootType.Add(
                            aggregateRootType,
                            new List<Type>(new[] {commandType}));
                    }
                    else
                    {
                        ((List<Type>) AggregateCommandsByAggregateRootType[aggregateRootType]).Add(commandType);
                    }
                }
            }
            
            SnapshotStateTypes = assembly
                .GetReferencedAssemblies()
                .Select(Assembly.Load)
                .Concat(new[] {assembly})
                .SelectMany(a => a.GetTypes())
                .Where(t => !t.GetTypeInfo().IsAbstract &&
                            typeof(IState).GetTypeInfo().IsAssignableFrom(t))
                .ToList();
        }

        private static IEnumerable<Type> GetAggregateRootTypes(Assembly assembly)
        {
            return GetTypesByInterface(
                assembly,
                typeof(IAggregateRoot));
        }
        
        private static IEnumerable<Type> GetProjectionTypes(Assembly assembly)
        {
            return GetTypesByInterface(
                assembly,
                typeof(IProjectionModel));
        }
        
        private static IEnumerable<Type> GetQueryRequestTypes(
            Assembly assembly,
            Type projectionModelType)
        {
            var queryRequestType = typeof(IQueryRequest<>).MakeGenericType(projectionModelType);
            return GetTypesByInterface(
                assembly,
                queryRequestType);
        }
        
        private static IEnumerable<Type> GetTypesByInterface(
            Assembly assembly,
            Type type)
        {
            return assembly
                .GetReferencedAssemblies()
                .Select(Assembly.Load)
                .Concat(new[] {assembly})
                .SelectMany(a => a.GetTypes())
                .Where(t => !t.GetTypeInfo().IsAbstract &&
                            type.GetTypeInfo().IsAssignableFrom(t))
                .ToList();
        }
        
        private static IEnumerable<Type> GetCommands(Assembly assembly)
        {
            return GetTypesByInterface(
                assembly,
                typeof(ICommand));
        }
    }
}