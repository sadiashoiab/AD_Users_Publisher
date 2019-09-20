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

                    // todo: add in environment variables, and pull the application insights key from it
                })
                .ConfigureLogging((hostingContext, builder) =>
                {
                    // todo: look to see if we can pull this key from Azure Key Vault
                    builder.AddApplicationInsights(hostingContext.Configuration.GetSection("ApplicationInsights")["InstrumentationKey"].ToString());
                    builder.AddFilter<ApplicationInsightsLoggerProvider>("", LogLevel.Debug);
                    builder.AddFilter<ApplicationInsightsLoggerProvider>("Microsoft", LogLevel.Debug);
                    builder.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                    builder.AddConsole();
                    builder.AddDebug();
                });
    }
}
