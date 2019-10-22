using System;
using System.Diagnostics.CodeAnalysis;
using Azure_AD_Users_Extract.Services;
using Azure_AD_Users_Shared.ExceptionFilters;
using Azure_AD_Users_Shared.Services;
using HealthChecks.UI.Client;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Polly;

namespace Azure_AD_Users_Extract
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

            var graphApiUrlFromConfig = Configuration["GraphApiUrl"];
            services.AddHealthChecks()
                .AddUrlGroup(new Uri(graphApiUrlFromConfig),
                    name: "GraphAPI URL",
                    failureStatus: HealthStatus.Unhealthy)
                .AddApplicationInsightsPublisher();

            services
                .AddHttpClient("GraphApiHttpClient",
                    client => { client.Timeout = System.Threading.Timeout.InfiniteTimeSpan; })
                .AddTransientHttpErrorPolicy(builder =>
                    builder.WaitAndRetryAsync(2, _ => TimeSpan.FromMilliseconds(500)));

            //services.AddMvc(options => { options.Filters.Add<ExceptionActionFilter>(); })
            //    .AddJsonOptions(options =>
            //    {
            //        var resolver  = options.SerializerSettings.ContractResolver;
            //        var res = (DefaultContractResolver) resolver;
            //        if (res != null)
            //        {
            //            res.NamingStrategy = null; // <-- this removes the default camelCasing of object property names when serializing to Json
            //        }
            //    })
            //    .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddControllers(configure =>
            {
                configure.Filters.Add<ExceptionActionFilter>(); 
            });

            services.AddSwaggerGen(c => {  
                c.SwaggerDoc("v1", new OpenApiInfo {  
                    Version = "v1",  
                    Title = "Azure_AD_Users_Extract API",  
                    Description = "Azure_AD_Users_Extract ASP.NET Core Web API"  
                });  
            });

            services.AddSingleton<IAzureKeyVaultService, AzureKeyVaultService>();
            services.AddSingleton<ITokenService, TokenService>();
            services.AddSingleton<IUserService, UserService>();
            services.AddSingleton<IGraphApiService, GraphApiService>();
            services.AddSingleton<IFranchiseUserService, FranchiseUserService>();

            services.AddHostedService<TopicClientHostedService>();
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
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Azure_AD_Users_Extract API V1");
            });
        }
    }
}
