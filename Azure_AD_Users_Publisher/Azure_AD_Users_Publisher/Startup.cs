using System;
using System.Diagnostics.CodeAnalysis;
using Azure_AD_Users_Publisher.Services;
using Azure_AD_Users_Shared.ExceptionFilters;
using Azure_AD_Users_Shared.Services;
using HealthChecks.UI.Client;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Polly;

namespace Azure_AD_Users_Publisher
{
    [ExcludeFromCodeCoverage]
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var appInsightServiceOptions = new ApplicationInsightsServiceOptions {EnableDebugLogger = true};
            services.AddApplicationInsightsTelemetry(appInsightServiceOptions);

            services.AddLazyCache();

            services.AddHealthChecks()
                .AddApplicationInsightsPublisher();

            services
                .AddHttpClient("TokenApiHttpClient",
                    client => { client.Timeout = System.Threading.Timeout.InfiniteTimeSpan; })
                .AddTransientHttpErrorPolicy(builder =>
                    builder.WaitAndRetryAsync(2, _ => TimeSpan.FromMilliseconds(500)));

            services
                .AddHttpClient("ProgramDataHttpClient",
                    client => { client.Timeout = System.Threading.Timeout.InfiniteTimeSpan; })
                .AddTransientHttpErrorPolicy(builder =>
                    builder.WaitAndRetryAsync(2, _ => TimeSpan.FromMilliseconds(500)));

            services
                .AddHttpClient("SalesforceHttpClient",
                    client => { client.Timeout = System.Threading.Timeout.InfiniteTimeSpan; })
                .AddTransientHttpErrorPolicy(builder =>
                    builder.WaitAndRetryAsync(2, _ => TimeSpan.FromMilliseconds(500)));

            services.AddControllers(configure =>
            {
                configure.Filters.Add<ExceptionActionFilter>(); 
            });

            services.AddSwaggerGen(c => {  
                c.SwaggerDoc("v1", new OpenApiInfo {  
                    Version = "v1",  
                    Title = "Azure_AD_Users_Publisher API",  
                    Description = "Azure_AD_Users_Publisher ASP.NET Core Web API"  
                });  
            });

            services.AddSingleton<IHISCTokenService, HISCTokenService>();
            services.AddSingleton<ISalesforceTokenService, SalesforceTokenService>();
            services.AddSingleton<IProgramDataService, ProgramDataService>();
            services.AddSingleton<IAzureKeyVaultService, AzureKeyVaultService>();
            services.AddSingleton<IMessageProcessor, SalesforceMessageProcessor>();
            services.AddSingleton<IGoogleApiService, GoogleApiService>();
            services.AddSingleton<ISalesforceUserService, SalesforceUserService>();
            services.AddSingleton<ITimeZoneService, TimeZoneService>();

            services.AddHostedService<SubscriptionClientHostedService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStatusCodePages();
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = _ => true,
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });

                endpoints.MapControllers();
            });

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Azure_AD_Users_Publisher API V1");
            });
        }
    }
}
