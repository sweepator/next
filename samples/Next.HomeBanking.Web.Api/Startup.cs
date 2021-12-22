using System;
using System.Net;
using System.Text.Json;
using Confluent.Kafka;
using DbUp;
using Hangfire;
using Hangfire.SqlServer;
using MassTransit;
using Hellang.Middleware.ProblemDetails;
using HotChocolate.Execution.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Next.Abstractions.Bus.Configuration;
using Next.Abstractions.EventSourcing;
using Next.Abstractions.EventSourcing.Metadata;
using Next.Abstractions.Serialization.Json;
using Next.Abstractions.Serialization.Metadata;
using Next.Cqrs.Configuration;
using Next.Application.Cqrs.Extensions;
using Next.HomeBanking.Application.Commands;
using Next.HomeBanking.Application.Queries;
using Next.HomeBanking.Domain.Events;
using Next.HomeBanking.Infrastructure.FluentValidation;
using Next.HomeBanking.Messaging;
using Next.Cqrs.Commands;
using Next.HomeBanking.Domain.Snapshots;
using Next.HomeBanking.Web.Api.Extensions;
using Next.HomeBanking.Web.Api.Mapping;
using Next.Web.Health;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Next.HomeBanking.Web.Api
{
    public class Startup
    {
        private IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            var connectionString = Configuration.GetConnectionString("SqlDatabase");
            var redisConnectionString = Configuration.GetConnectionString("RedisBus");
            var mongoDbConnectionString = Configuration.GetConnectionString("MongoDb");
            var kafkaHost = Configuration.GetConnectionString("Kafka");
            var applicationAssembly = typeof(CreateAccountCommand).Assembly;
            var fluentValidatorsAssembly = typeof(CreateBankAccountCommandValidator).Assembly;
            
            // ensures sql database is created, before applying hangfire db scripts
            if (!string.IsNullOrWhiteSpace(Configuration["resetdb"]))
            {
                DropDatabase.For.SqlDatabase(connectionString);
            }
            
            EnsureDatabase.For.SqlDatabase(connectionString);
            
            #region serialization metadata configuration
            
            var metadataProviderBuilder = new SerializerMetadataProviderBuilder();
            metadataProviderBuilder
                .AddProfilesFrom(applicationAssembly)
                .IgnoreBaseCommandResponseProperties();
            services.AddSingleton(metadataProviderBuilder.SerializerMetadataProvider);
            var serializerMetadataProvider = metadataProviderBuilder.SerializerMetadataProvider;
            
            #endregion

            #region problemdetails configuration

            services
                .AddProblemDetailsDefaults(o =>
                {
                    o.AddProfiles(GetType().Assembly);
                });
            
            #endregion
            
            #region hypermedia configuration

            services
                .AddHypermedia(
                    GetType().Assembly,
                    o => o.ConfigureHypermedia());
            
            #endregion

            #region core configuration

            var mvcCoreBuilder = services
                .AddHttpContextAccessor()
                .AddSingleton<IActionContextAccessor, ActionContextAccessor>()
                .Configure<ApiBehaviorOptions>(o =>
                {
                    // disable default model binding validation
                    o.SuppressModelStateInvalidFilter = true;
                })
                .AddMvcCore(o =>
                {
                    o.RespectBrowserAcceptHeader = true;
                    //o.ReturnHttpNotAcceptable = true;

                    o.AddHypermediaFormatters()
                        .AddBodyAndRouteBinding()
                        .AddSingleValueBinders();
                })
                .AddControllersAsServices();
            
            #endregion
                
            #region json configuration

            services
                .AddJsonSerializer(o => o.AddMetadataProvider(serializerMetadataProvider));

            mvcCoreBuilder
                .AddJsonOptions(o => o
                    .JsonSerializerOptions
                    .AddDefaults()
                    .AddMetadataProvider(smb => smb
                        .IgnoreBaseCommandResponseProperties())
                    .AddDomainConverters());
            
            #endregion

            #region mapping configuration

            services
                .AddMaping(new CommandProfile());
            
            #endregion
            
            #region api version configuration

            services
                .AddApiVersionDefaults();
            
            #endregion

            #region swagger configuration

            services
                .AddSwaggerGenerationConfiguration(
                    GetType().Assembly,
                    Consts.ApplicationName,
                    o =>
                    {
                        o.AddMetadataProvider(serializerMetadataProvider);
                        o.AddCqrsFilters(applicationAssembly);
                    });

            #endregion
            
            #region fluent validaton configuration

            services
                .AddValidators(fluentValidatorsAssembly)
                .AddAggregateValidators();
            
            #endregion
            
            #region data migrations configuration

            services
                .AddHomeBankingDataMigrations(o =>
                    o.WitConnectionString(connectionString));
            
            #endregion

            #region masstransit configuration

            services
                .AddMassTransit(o =>
                {
                    o.AddMessageBusTransport();
                    o.AddConsumer<BankAccountCreatedConsumer>();
                    o.UsingRabbitMq((context, cfg) =>
                    {
                        cfg.ConfigureMessageBusTransport(context);

                        cfg.Publish<BankAccountCreated>(x => x.Exclude = true);
                        /*cfg.ReceiveEndpoint("domain-event-listener", e =>
                        {
                            e.ConfigureConsumer<BankAccountCreatedConsumer>(context);
                        });*/
                    });
                })
                .AddMassTransitHostedService();
            
            #endregion
            
            #region hangfire configuration

            services
                .AddHangfireJobScheduler()
                .AddHangfireServerDefaults(Consts.ApplicationName)
                .AddHangfire(c => c
                    .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                    .UseSimpleAssemblyNameTypeSerializer()
                    .UseRecommendedSerializerSettings()
                    .UseSqlServerStorage(connectionString, new SqlServerStorageOptions
                    {
                        CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                        SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                        QueuePollInterval = TimeSpan.Zero,
                        UseRecommendedIsolationLevel = true,
                        DisableGlobalLocks = true,
                        PrepareSchemaIfNecessary = true,

                    }))
                .AddHangfireServer(c =>
                {
                    c.Queues = new[] {"default"};
                    c.ServerName = $"{Consts.ApplicationName}.{Dns.GetHostName()}";
                });
            
            #endregion

            #region cqrs + es configuration

            var cqrsBuilder = services
                .AddCqrs(
                    applicationAssembly,
                    cqrs => cqrs
                        .Bus(bus => bus
                            .WithMessageJsonSerializer()
                            /*.UseKafka(
                                kafkaHost,
                                cqrs.DomainMetadataInfo)*/
                            .UseMassTransit(cqrs.DomainMetadataInfo)
                        )
                        .EventStore(esb => esb
                            .UseJsonSerializer()
                            .AddMetadataProvider<MachineNameMetadataEnricher>()
                            .AddSnapshotStrategy<BankAccountSnapshotStrategy>()
                            .UseSqlServer(connectionString)
                            .ConfigurePublisher(epo =>
                            {
                                epo.BackgroundProcessorEnabled = false;
                                epo.InlineProcessorEnabled = true;
                                epo.BackgroundLockInSeconds = 2;
                            }))
                        .Projections(pb =>
                        {
                            pb.UseSqlServer(connectionString)
                                .SyncProjection<BankAccountIndexProjection>(o =>
                                {
                                    o.Enabled = false;
                                });

                            pb.UseMongoDb(
                                    mongoDbConnectionString,
                                    "HomeBanking")
                                .AsyncProjection<BankAccountProjection>(o =>
                                {
                                    o.Enabled = true;
                                    o.Rebuild = true;
                                })
                                .AsyncProjection<BankAccountTransactionProjection,
                                    BankAccountTransactionProjectionLocator>(o =>
                                {
                                    o.Enabled = true;
                                    o.Rebuild = true;
                                })
                                .AsyncProjection<BankAccountDetailsProjection>(o =>
                                {
                                    o.Enabled = true;
                                    o.Rebuild = true;
                                });
                        })
                        .Scheduler(s =>
                        {
                            s.CommandFor<ProcessAccountCommand, CommandResponse, BankAccountCreated>(o =>
                                o.Delay = TimeSpan.FromSeconds(10));
                        }));
            
            #endregion
            
            #region application pipeline/steps configuration

            services
                .AddHttpPortAdapter()
                .AddLogStep(o => o.Config<GetAccountsRequest>(l => l.LogResponse = false))
                .AddValidationStep();
            
            #endregion

            #region health checks configuration

            services
                .AddHealthChecks()
                .AddStartupHealthCheck()
                .AddEventStoreHealthCheck(
                    tags: HealthTags.InfoTags)
                .AddCatalogDataMigrationsHealthCheck(
                    tags: HealthTags.InfoTags)
                .AddSqlServer(
                    connectionString,
                    timeout: TimeSpan.FromSeconds(1),
                    tags: HealthTags.ReadyTags)
                .AddRedis(
                    redisConnectionString,
                    timeout: TimeSpan.FromSeconds(1),
                    tags: HealthTags.ReadyTags)
                .AddMongoDb(
                    mongoDbConnectionString,
                    timeout: TimeSpan.FromSeconds(1),
                    tags: HealthTags.ReadyTags)
                .AddKafka(
                    new ProducerConfig
                    {
                        BootstrapServers = kafkaHost
                    },
                    timeout: TimeSpan.FromSeconds(1),
                    tags: HealthTags.ReadyTags);

            #endregion
            
            #region graphql configuration

            services
                .AddGraphQLServer()
                .AddInMemorySubscriptions()
                .AddCqrs(cqrsBuilder.DomainMetadataInfo);

            #endregion

            #region integration events

            /*services.AddKafkaDomainIntegration<BankAccountCreated, BankAccountCreatedIntegrationEvent>(
                k =>
                {
                    k.BootstrapServers = kafkaHost;
                    k.AutoCreateTopic = true;
                },
                o =>
                    o.MapFunc =
                        domainEvent => new BankAccountCreatedIntegrationEvent()
                        {
                            Id = domainEvent.AggregateIdentity.Value
                        });*/

            #endregion
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(
            IApplicationBuilder app, 
            IWebHostEnvironment env,
            IApiVersionDescriptionProvider apiVersionDescriptionProvider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseProblemDetails();
            
            app.UseRouting();
            
            app.UseWebSockets();

            // swagger settings
            app.UseSwaggerDefaults(apiVersionDescriptionProvider);
            
            //hangfire
            app.UseHangfireDashboard("/dashboard");

            // redoc settings
            app.UseReDoc();
            
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecksDefaults();
                endpoints.MapControllers();
                endpoints.MapGraphQL();
            });
        }
    }
}
    