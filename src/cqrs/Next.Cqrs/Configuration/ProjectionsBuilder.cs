using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Next.Abstractions.Domain;
using Next.Abstractions.EventSourcing;
using Next.Cqrs.Queries;
using Next.Cqrs.Queries.Memory;
using Next.Cqrs.Queries.Projections;
using Next.Cqrs.Queries.Services;

namespace Next.Cqrs.Configuration
{
    internal class ProjectionsBuilder : IProjectionsBuilder
    {
        public IServiceCollection Services { get; }
        public IDictionary<Type, IEnumerable<Type>> QueryRequestByProjectionTypes { get; }

        private static readonly MethodInfo UseQueryStoreManagerSingleMethod =
            typeof(ProjectionsBuilder)
                .GetTypeInfo()
                .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                .Single(m => m.Name == nameof(QueryStore) && m.GetGenericArguments().Length == 4);
        
        private static readonly MethodInfo UseQueryStoreManagerMultipleMethod =
            typeof(ProjectionsBuilder)
                .GetTypeInfo()
                .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                .Single(m => m.Name == nameof(QueryStore) && m.GetGenericArguments().Length == 5);

        public ProjectionsBuilder(
            IServiceCollection services,
            IDictionary<Type, IEnumerable<Type>> queryRequestByProjectionTypes)
        {
            Services = services;
            QueryRequestByProjectionTypes = queryRequestByProjectionTypes;
        }
        
        public IProjectionsBuilder QueryStoreFor<TQueryStoreService, TQueryStoreImplementation, TProjectionModel>(
            bool isSync = false,
            Action<QueryStorePopulatorStartupOptions<TProjectionModel>> setup = null)
            where TProjectionModel : class, IProjectionModel
            where TQueryStoreService : class, IQueryStore<TProjectionModel>
            where TQueryStoreImplementation : class, TQueryStoreService
        {
            QueryStoreFor<TQueryStoreService, TProjectionModel>(isSync);
            Services.AddTransient<TQueryStoreService, TQueryStoreImplementation>();
            Services.AddTransient<IQueryStore<TProjectionModel>>(r => r.GetRequiredService<TQueryStoreService>());
            Services.AddTransient<IQueryStore>(sp => sp.GetRequiredService<TQueryStoreService>());

            setup ??= _ => { };
            Services
                .AddStartupTask<QueryStorePopulatorStartupTask<TProjectionModel>>()
                .Configure(setup);
            
            return this;
        }
        
        public IProjectionsBuilder QueryStoreFor<TQueryStoreService, TQueryStoreImplementation, TProjectionModel, TProjectionModelLocator>(
            bool isSync = false,
            Action<QueryStorePopulatorStartupOptions<TProjectionModel>> setup = null)
            where TProjectionModel : class, IProjectionModel
            where TQueryStoreService : class, IQueryStore<TProjectionModel>
            where TQueryStoreImplementation : class, TQueryStoreService
            where TProjectionModelLocator : class, IProjectionModelLocator
        {
            QueryStoreFor<TQueryStoreService, TProjectionModel, TProjectionModelLocator>(isSync);
            Services.AddTransient<TQueryStoreService, TQueryStoreImplementation>();
            Services.AddTransient<IQueryStore<TProjectionModel>>(r => r.GetRequiredService<TQueryStoreService>());
            Services.AddTransient<IQueryStore>(sp => sp.GetRequiredService<TQueryStoreService>());

            setup ??= _ => { };
            Services
                .AddStartupTask<QueryStorePopulatorStartupTask<TProjectionModel>>()
                .Configure(setup);
            
            return this;
        }
        
        public IProjectionsBuilder InMemoryQueryStoreFor<TProjectionModel>(Action<QueryStorePopulatorStartupOptions<TProjectionModel>> setup = null)
            where TProjectionModel : class, IProjectionModel
        {
            QueryStoreFor<IQueryableQueryStore<TProjectionModel>, TProjectionModel>();
            Services.AddSingleton<IQueryableQueryStore<TProjectionModel>, InMemoryQueryStore<TProjectionModel>>();
            Services.AddSingleton<IQueryStore<TProjectionModel>>(sp => sp.GetRequiredService<IQueryableQueryStore<TProjectionModel>>());
            Services.AddSingleton<IQueryStore>(sp => sp.GetRequiredService<IQueryableQueryStore<TProjectionModel>>());
            AddQueryService<TProjectionModel>(typeof(QueryableService<,>));

            setup ??= _ => { };
            Services
                .AddStartupTask<QueryStorePopulatorStartupTask<TProjectionModel>>()
                .Configure(setup);
            
            return this;
        }
        
        public IProjectionsBuilder InMemoryQueryStoreFor<TProjectionModel, TProjectionModelLocator>(Action<QueryStorePopulatorStartupOptions<TProjectionModel>> setup = null)
            where TProjectionModel : class, IProjectionModel
            where TProjectionModelLocator : class, IProjectionModelLocator
        {
            QueryStoreFor<IQueryableQueryStore<TProjectionModel>, TProjectionModel, TProjectionModelLocator>();
            Services.AddSingleton<IQueryableQueryStore<TProjectionModel>, InMemoryQueryStore<TProjectionModel>>();
            Services.AddSingleton<IQueryStore<TProjectionModel>>(sp => sp.GetRequiredService<IQueryableQueryStore<TProjectionModel>>());
            Services.AddSingleton<IQueryStore>(sp => sp.GetRequiredService<IQueryableQueryStore<TProjectionModel>>());
            AddQueryService<TProjectionModel>(typeof(QueryableService<,>));

            setup ??= _ => { };
            Services
                .AddStartupTask<QueryStorePopulatorStartupTask<TProjectionModel>>()
                .Configure(setup);
            
            return this;
        }

        public IProjectionsBuilder AddQueryService<TProjectionModel>(Type queryServiceType) 
            where TProjectionModel : class, IProjectionModel
        {
            if (QueryRequestByProjectionTypes.TryGetValue(
                typeof(TProjectionModel),
                out var queryRequestTypes))
            {
                foreach (var queryRequestType in queryRequestTypes)
                {
                    var queryServiceInterface = typeof(IQueryService<,>).MakeGenericType(
                        queryRequestType, 
                        typeof(TProjectionModel));
                    var queryServiceImplementation = queryServiceType.MakeGenericType(
                        queryRequestType, 
                        typeof(TProjectionModel));
                    
                    var queryHandlerType = typeof(QueryHandler<,>).MakeGenericType(
                        queryRequestType, 
                        typeof(TProjectionModel));
                    var queryHandlerInterfaceType = typeof(MediatR.IRequestHandler<,>).MakeGenericType(
                        queryRequestType, 
                        typeof(IQueryResponse<>).MakeGenericType(typeof(TProjectionModel)));
                    
                    // register query service
                    Services
                        .TryAddTransient(queryServiceInterface, queryServiceImplementation);
                    
                    // register query handler
                    Services
                        .TryAddTransient(queryHandlerInterfaceType, queryHandlerType);
                }
            }

            return this;
        }

        public IProjectionsBuilder PopulatorOptions(Action<QueryStorePopulatorOptions> setup)
        {
            Services.Configure(setup);
            return this;
        }

        /*public static IProjectionsBuilder UseInMemoryProjectionStoreFor<TProjectionModel, TProjectionModelLocator>(
            this IProjectionsBuilder projectionsBuilder)
            where TProjectionModel : class, IProjectionModel
            where TProjectionModelLocator : IProjectionModelLocator
        {
            return projectionsBuilder
                .UseQueryStoreManagerFor<IInMemoryQueryStore<TProjectionModel>, InMemoryQueryStore<TProjectionModel>, TProjectionModel>();
        }*/

        /*public IProjectionsBuilder UseQueryStoreFor<TProjectionStore, TProjectionModel, TProjectionModelLocator>() 
            where TProjectionStore : class, IQueryStore<TProjectionModel> 
            where TProjectionModel : class, IProjectionModel 
            where TProjectionModelLocator : IProjectionModelLocator
        {
            Services
                .AddTransient<IQueryStoreManager, MultipleQueryStoreManager<TProjectionStore, TProjectionModel, TProjectionModelLocator>>();
            //.AddTransient<IQueryHandler<ReadModelByIdQuery<TProjectionModel>, TProjectionModel>, ReadModelByIdQueryHandler<TProjectionStore, TProjectionModel>>();
                
            return this;
        }*/

        private IProjectionsBuilder QueryStore<TAggregate, TIdentity, TProjectionModelStore, TProjectionModel>(
            bool isSync = false)
            where TAggregate : IAggregateRoot<TIdentity>
            where TIdentity : IIdentity
            where TProjectionModelStore : class, IQueryStore<TProjectionModel>
            where TProjectionModel : class, IProjectionModel
        {
            if (isSync)
            {
                Services
                    .AddTransient<SingleQueryStoreUpdater<TAggregate, TIdentity, TProjectionModelStore,
                        TProjectionModel>>()
                    .AddTransient<ISyncQueryStoreUpdater, SingleQueryStoreUpdater<TAggregate, TIdentity,
                        TProjectionModelStore, TProjectionModel>>()
                    .AddTransient<IEventStoreSyncProcessor, QueryStoreEventStoreSyncProcessor<SingleQueryStoreUpdater<
                        TAggregate, TIdentity, TProjectionModelStore, TProjectionModel>>>();

                return this;
            }

            Services
                .AddTransient<SingleQueryStoreUpdater<TAggregate, TIdentity, TProjectionModelStore, TProjectionModel>>()
                .AddTransient<IAsyncQueryStoreUpdater, SingleQueryStoreUpdater<TAggregate, TIdentity,
                    TProjectionModelStore, TProjectionModel>>();


            return this;
        }
        
        private IProjectionsBuilder QueryStore<TAggregate, TIdentity, TProjectionModelStore, TProjectionModel, TProjectionModelLocator>(
            bool isSync = false)
            where TAggregate : IAggregateRoot<TIdentity>
            where TIdentity : IIdentity
            where TProjectionModelStore : class, IQueryStore<TProjectionModel>
            where TProjectionModel : class, IProjectionModel
            where TProjectionModelLocator : class, IProjectionModelLocator
        {
            if (isSync)
            {
                Services
                    .AddSingleton<TProjectionModelLocator>()
                    .AddTransient<MultipleQueryStoreUpdater<TProjectionModelStore, TProjectionModel, TProjectionModelLocator>>()
                    .AddTransient<ISyncQueryStoreUpdater, MultipleQueryStoreUpdater<TProjectionModelStore, TProjectionModel, TProjectionModelLocator>>()
                    .AddTransient<IEventStoreSyncProcessor, QueryStoreEventStoreSyncProcessor<SingleQueryStoreUpdater<TAggregate, TIdentity, TProjectionModelStore, TProjectionModel>>>();

                return this;
            }

            Services
                .AddSingleton<TProjectionModelLocator>()
                .AddTransient<MultipleQueryStoreUpdater<TProjectionModelStore, TProjectionModel, TProjectionModelLocator>>()
                .AddTransient<IAsyncQueryStoreUpdater, MultipleQueryStoreUpdater<TProjectionModelStore, TProjectionModel, TProjectionModelLocator>>();

            return this;
        }

        private void QueryStoreFor<TQueryStore, TProjectionModel>(bool isSync = false) 
            where TQueryStore : class, IQueryStore<TProjectionModel> 
            where TProjectionModel : class, IProjectionModel
        {
            var (aggregateType, idType) = GetSingleAggregateTypes<TProjectionModel>();
            
            UseQueryStoreManagerSingleMethod
                .MakeGenericMethod(aggregateType, idType, typeof(TQueryStore), typeof(TProjectionModel))
                .Invoke(this, 
                    new object[]
                    {
                        isSync
                    });
        }
        
        private void QueryStoreFor<TQueryStore, TProjectionModel, TProjectionModelLocator>(bool isSync = false) 
            where TQueryStore : class, IQueryStore<TProjectionModel> 
            where TProjectionModel : class, IProjectionModel
            where TProjectionModelLocator: class, IProjectionModelLocator
        {
            var (aggregateType, idType) = GetSingleAggregateTypes<TProjectionModel>();
            
            UseQueryStoreManagerMultipleMethod
                .MakeGenericMethod(aggregateType, idType, typeof(TQueryStore), typeof(TProjectionModel), typeof(TProjectionModelLocator))
                .Invoke(this, 
                    new object[]
                    {
                        isSync
                    });
        }
        
        private static (Type aggregateType, Type idType) GetSingleAggregateTypes<TProjectionModel>()
            where TProjectionModel : class, IProjectionModel
        {
            var readModelInterface = typeof(IProjectionModelFor<,,>);

            bool IsReadModelInterface(Type type)
            {
                var info = type.GetTypeInfo();
                if (!info.IsGenericType)
                {
                    return false;
                }
                
                var definition = info.GetGenericTypeDefinition();
                return definition == readModelInterface;
            }

            var readModelType = typeof(TProjectionModel);
            var results = readModelType
                .GetTypeInfo()
                .GetInterfaces()
                .Where(IsReadModelInterface)
                .GroupBy(i => new
                {
                    AggregateType = i.GenericTypeArguments[0],
                    IdType = i.GenericTypeArguments[1]
                })
                .ToList();

            if (!results.Any())
            {
                var message = $"You are trying to register ProjectionModel type {typeof(TProjectionModel).Name} which doesn't subscribe to any events. Implement the IProjectionModelFor<,,> interface.";
                throw new InvalidOperationException(message);
            }

            if (results.Count > 1)
            {
                var message = $"You are trying to register ProjectionModel type {typeof(TProjectionModel).Name} " +
                              "which subscribes to events from different aggregates. " +
                              "Use a ProjectionModelLocator, like this: " +
                              $"options.UseSomeReadStoreFor<{typeof(TProjectionModel)},MyReadModelLocator>";

                throw new InvalidOperationException(message);
            }

            var result = results.Single();
            return (result.Key.AggregateType, result.Key.IdType);
        }
    }
}