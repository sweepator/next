using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Next.Web.Health;

namespace Microsoft.AspNetCore.Builder
{
    public static class HealthCheckExtensions
    {
        private static async Task WriteResponse(
            HttpContext context,
            HealthReport result)
        {
            context.Response.ContentType = "application/json; charset=utf-8";

            var options = new JsonWriterOptions
            {
                Indented = true,
            };

            await using var stream = new MemoryStream();
            await using (var writer = new Utf8JsonWriter(stream, options))
            {
                writer.WriteStartObject();
                writer.WriteString("status", result.Status.ToString());

                if (result.Entries.Any())
                {
                    writer.WriteStartObject("results");

                    foreach (var (key, value) in result.Entries)
                    {
                        writer.WriteStartObject(key);
                        writer.WriteString("status", value.Status.ToString());

                        if (!string.IsNullOrEmpty(value.Description))
                        {
                            writer.WriteString("description", value.Description);
                        }

                        writer.WriteNumber("duration", value.Duration.TotalMilliseconds);
                        
                        if (value.Exception != null)
                        {
                            writer.WriteStartObject("error");
                            writer.WriteString("message", value.Exception.Message);
                            writer.WriteString("details", value.Exception.StackTrace);
                            writer.WriteEndObject();
                        }

                        if (value.Data.Any())
                        {
                            writer.WriteStartObject("data");
                            foreach (var (s, o) in value.Data)
                            {
                                writer.WritePropertyName(s);
                                JsonSerializer.Serialize(writer, o);
                            }

                            writer.WriteEndObject();
                        }

                        writer.WriteEndObject();
                    }

                    writer.WriteEndObject();
                }

                writer.WriteEndObject();
            }

            var json = Encoding.UTF8.GetString(stream.ToArray());
            await context.Response.WriteAsync(json);
        }

        public static IEndpointConventionBuilder MapHealthChecksDefaults(this IEndpointRouteBuilder endpoints,
            string readyRoute = "/health/ready",
            string liveRoute = "/health/live",
            string infoRoute = "/health/info")
        {
            endpoints.MapHealthChecks(
                infoRoute,
                new HealthCheckOptions
                {
                    AllowCachingResponses = false,
                    ResponseWriter = WriteResponse,
                    Predicate = (check) => check.Tags.Contains(HealthTags.InfoTag),
                });
            
            endpoints.MapHealthChecks(
                readyRoute,
                new HealthCheckOptions
                {
                    AllowCachingResponses = false,
                    ResponseWriter = WriteResponse,
                    Predicate = (check) => check.Tags.Contains(HealthTags.ReadyTag),
                });

            return endpoints
                .MapHealthChecks(
                    liveRoute,
                    new HealthCheckOptions
                    {
                        AllowCachingResponses = false,
                        ResponseWriter = WriteResponse,
                        Predicate = _ => false,
                    });
        }
    }
}
