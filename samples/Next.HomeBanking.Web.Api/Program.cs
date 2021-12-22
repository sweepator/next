using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Next.HomeBanking.Web.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Serilog.Log.Logger = HostBuilderExtensions.GetDefaultStartupLogger(true);

            try
            {
                Serilog.Log.Information("Starting up");
                CreateHostBuilder(args)
                    .Build()
                    .Run();
            }
            catch (Exception ex)
            {
                Serilog.Log.Fatal(ex, "Application start-up failed");   
            }
            finally
            {
                Serilog.Log.CloseAndFlush();
            }
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilogDefaults(o =>
                {
                    o.ApplicationName = Consts.ApplicationName;
                    o.LogToStandardOutput = true;
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
