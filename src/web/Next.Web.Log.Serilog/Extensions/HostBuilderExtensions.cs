using Next.Abstractions.Log;
using Serilog;
using Serilog.Configuration;
using Serilog.Sinks.Elasticsearch;
using Serilog.Sinks.SystemConsole.Themes;
using System;
using Serilog.Events;
using Serilog.Formatting.Elasticsearch;

namespace Microsoft.Extensions.Hosting
{
    public static class HostBuilderExtensions
    {
        private static ElasticsearchSinkOptions GetElasticsearchSinkOptions(
            Uri uri,
            TimeSpan connectionTimeout,
            string bufferBaseFilename,
            string indexFormat)
        {
            return new ElasticsearchSinkOptions(uri)
            {
                AutoRegisterTemplate = true,
                EmitEventFailure = EmitEventFailureHandling.WriteToSelfLog |
                                       EmitEventFailureHandling.WriteToFailureSink |
                                       EmitEventFailureHandling.RaiseCallback,
                BufferBaseFilename = bufferBaseFilename,
                BufferFileSizeLimitBytes = 10485760,
                BufferFileCountLimit = 5,
                ConnectionTimeout = connectionTimeout,
                IndexFormat = indexFormat
            };
        }

        public static IHostBuilder UseSerilogDefaults(
            this IHostBuilder builder,
            Action<SerilogOptions> setup = null)
        {
            var options = new SerilogOptions();
            setup?.Invoke(options);
            
            return builder
                .UseSerilog((hostingContext, serviceProvider, loggerConfiguration) =>
                    {
                        loggerConfiguration
                            .ReadFrom
                            .Configuration(hostingContext.Configuration)
                            .Enrich.FromLogContext()
                            .Enrich.WithProperty("ApplicationName", options.ApplicationName)
                            .Enrich.WithServiceProvider(serviceProvider)
                            .Enrich.WithThreadId();

                        options.OnConfigure?.Invoke(loggerConfiguration);

                        if (Uri.IsWellFormedUriString(hostingContext.Configuration["ElasticConfiguration:Uri"],
                            UriKind.Absolute))
                        {
                            var uri = new Uri(hostingContext.Configuration["ElasticConfiguration:Uri"]);
                            var connectionTimeout = TimeSpan.FromMilliseconds(20);

                            loggerConfiguration
                                .WriteTo.Logger(lc =>
                                {
                                    lc.Filter.ByIncludingOnly($"{nameof(Properties.LogCategory)} is null")
                                        .WriteTo.Elasticsearch(GetElasticsearchSinkOptions(
                                            uri,
                                            connectionTimeout,
                                            $".//Logs//{options.ApplicationName.ToLower()}",
                                            $"{options.ApplicationName}{DateTime.UtcNow:yyyy-MM}"));
                                });

                            foreach (var logCategory in options.LogCategories)
                            {
                                loggerConfiguration.WriteTo.Logger(lc =>
                                {
                                    lc.Filter.ByIncludingOnly($"{nameof(Properties.LogCategory)} = '{logCategory}'")
                                        .WriteTo.Elasticsearch(GetElasticsearchSinkOptions(
                                            uri,
                                            connectionTimeout,
                                            $".//Logs//{options.ApplicationName.ToLower()}-{logCategory}'",
                                            $"{options.ApplicationName}-{logCategory}{DateTime.UtcNow:yyyy-MM}"));
                                });
                            }
                        }

                        if (!options.LogToStandardOutput)
                        {
                            return;
                        }

                        if (hostingContext.HostingEnvironment.IsLocal())
                        {
                            loggerConfiguration
                                .WriteTo.Console(
                                    outputTemplate:
                                    options.LocalOutputTemplate ??
                                    "{Timestamp:yyyy-MM-ddTHH:mm:ss.fffzzz} [{ThreadId}] {Level:u4} - {SourceContext} - {RequestId} - {Message}{NewLine}{Exception}",
                                    theme: AnsiConsoleTheme.Code);
                        }
                        else
                        {
                            loggerConfiguration.WriteTo.Console(new ElasticsearchJsonFormatter());
                        }
                    },
                    true);
        }
        
        public static ILogger GetDefaultStartupLogger(bool logToStandardOutput = false)
        {
            var configuration = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .Enrich.FromLogContext();

            if (!logToStandardOutput)
            {
                return configuration.CreateLogger();
            }
            
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? string.Empty;

            if (environment.Equals("Local", StringComparison.InvariantCultureIgnoreCase))
            {
                configuration
                    .WriteTo.Console(
                        outputTemplate: "{Timestamp:yyyy-MM-ddTHH:mm:ss.fffzzz} [{ThreadId}] {Level:u4} - {Message}{NewLine}{Exception}",
                        theme: AnsiConsoleTheme.Code);
                return configuration.CreateLogger();
            }
            
            configuration.WriteTo.Console(new ElasticsearchJsonFormatter());
            return configuration.CreateLogger();
        }
    }
}
