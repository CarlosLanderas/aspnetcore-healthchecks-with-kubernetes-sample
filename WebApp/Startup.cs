using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace WebApp
{
    public class Startup
    {
        bool AppHealthy = true;
        bool ServiceBusHealth = true;
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHealthChecks()
                .AddCheck("self", c =>
                {
                    return AppHealthy ? HealthCheckResult.Healthy() : HealthCheckResult.Unhealthy();
                })
                .AddSqlServer("server=sqlserver;initial catalog=master;user id=sa;password=Passw0rd!", tags: new[] { "services" })
                .AddRedis("redis", tags: new[] { "services" })
                .AddCheck("ServiceBus", c =>
                {
                    return ServiceBusHealth ? HealthCheckResult.Healthy() : HealthCheckResult.Unhealthy();
                }, tags: new[] { "services" });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseHealthChecks("/health-ui", new HealthCheckOptions
            {
                Predicate = registration => true,
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });

            app.UseHealthChecks("/self", new HealthCheckOptions
            {
                Predicate = registration => registration.Name == "self"
            });

            app.UseHealthChecks("/ready", new HealthCheckOptions
            {
                Predicate = registration => registration.Tags.Contains("services")
            });

            app.Map("/switch", appBuilder =>
            {
                appBuilder.Run(async context =>
                {
                    AppHealthy = !AppHealthy;
                    await context.Response.WriteAsync($"{Environment.MachineName} health status changed to {AppHealthy}");
                });
            });

            app.Map("/switch-service", appBuilder =>
            {
                appBuilder.Run(async context =>
                {
                    ServiceBusHealth = !ServiceBusHealth;
                    await context.Response.WriteAsync($"{Environment.MachineName} service bus health status changed to {ServiceBusHealth}");
                });
            });

            app.Run(async context =>
            {
                await context.Response.WriteAsync($"Hello from {Environment.MachineName}");
            });
        }
    }

}
