using System;
using System.Diagnostics.CodeAnalysis;
using Azure_AD_Users_Extract.Filters;
using Azure_AD_Users_Extract.Services;
using Azure_AD_Users_Extract.Services.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Swashbuckle.AspNetCore.Swagger;

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
            services
                .AddHttpClient("GraphApiHttpClient",
                    client => { client.Timeout = System.Threading.Timeout.InfiniteTimeSpan; })
                .AddTransientHttpErrorPolicy(builder =>
                    builder.WaitAndRetryAsync(2, _ => TimeSpan.FromMilliseconds(500)));
            services
                .AddHttpClient("GoogleApiHttpClient",
                    client => { client.Timeout = System.Threading.Timeout.InfiniteTimeSpan; })
                .AddTransientHttpErrorPolicy(builder =>
                    builder.WaitAndRetryAsync(2, _ => TimeSpan.FromMilliseconds(500)));

            services.AddMvc(options => { options.Filters.Add<ExceptionActionFilter>(); })
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddSwaggerGen(c => {  
                c.SwaggerDoc("v1", new Info {  
                    Version = "v1",  
                    Title = "AD_Users_Extract API",  
                    Description = "AD_Users_Extract ASP.NET Core Web API"  
                });  
            });

            services.AddApplicationInsightsTelemetry();

            services.AddScoped<IAzureKeyVaultService, AzureKeyVaultService>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IGraphApiService, GraphApiService>();
            services.AddScoped<IGoogleApiService, GoogleApiService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
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
            app.UseMvc();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "AD_Users_Extract API V1");
            });
        }
    }
}
