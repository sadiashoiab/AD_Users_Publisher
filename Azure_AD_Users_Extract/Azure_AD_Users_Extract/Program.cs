using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.ApplicationInsights;

namespace Azure_AD_Users_Extract
{
    [ExcludeFromCodeCoverage]
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    var environment = hostingContext.HostingEnvironment;
                    config.AddJsonFile("appsettings.json", false, true);
                    // note: set ASPNETCORE_ENVIRONMENT environment variable to pull in environment specific configurations.
                    //       depending on what OS you are deploying to, this CAN be case sensitive
                    config.AddJsonFile($"appsettings.{environment.EnvironmentName}.json", true, true);
                    config.AddEnvironmentVariables("APPLICATION_");
                })
                .ConfigureLogging((hostingContext, builder) =>
                {
                    builder.AddApplicationInsights(hostingContext.Configuration.GetSection("AI_KEY").Value);
                    builder.AddFilter<ApplicationInsightsLoggerProvider>("", LogLevel.Debug);
                    builder.AddFilter<ApplicationInsightsLoggerProvider>("Microsoft", LogLevel.Debug);
                    builder.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                    builder.AddConsole();
                    builder.AddDebug();
                });
    }
}
