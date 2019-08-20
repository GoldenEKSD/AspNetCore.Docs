﻿using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SampleApp.Data;

namespace SampleApp
{
    // Use the `--scenario dbcontext` switch to run this version of the sample.
    //
    // Register Health Check Middleware at the URL: /health
    // 
    // By default, health checks return a 200-Ok with 'Healthy' when the database is responsive.
    // - A a SqlConnectionHealthCheck is used in the example for a SQL database.
    // - The default response writer writes the HealthStatus as text/plain content.
    //
    // AddDbContextCheck<TContext> registers a health check for the TContext type. By default, the name of the health check is the name of the TContext type. There are other options available through AddDbContextCheck to configure failure status, tags, and a custom test query.

    public class DbContextHealthStartup
    {
        public DbContextHealthStartup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            #region snippet_ConfigureServices
            services.AddHealthChecks()
                .AddDbContextCheck<AppDbContext>();

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlServer(
                    Configuration["ConnectionStrings:DefaultConnection"]);
            });
            #endregion
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseRouting();

            #region snippet_Configure
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/health");
            });
            #endregion

            app.Map("/createdatabase", b => b.Run(async (context) =>
            {
                await context.Response.WriteAsync("Creating the database...");
                await context.Response.WriteAsync(Environment.NewLine);
                await context.Response.Body.FlushAsync();

                var appDbContext = 
                    context.RequestServices.GetRequiredService<AppDbContext>();
                await appDbContext.Database.EnsureCreatedAsync();

                await context.Response.WriteAsync("Done!");
                await context.Response.WriteAsync(Environment.NewLine);
                await context.Response.WriteAsync(
                    "Navigate to /health to see the health status.");
                await context.Response.WriteAsync(Environment.NewLine);
            }));

            app.Map("/deletedatabase", b => b.Run(async (context) =>
            {
                await context.Response.WriteAsync("Deleting the database...");
                await context.Response.WriteAsync(Environment.NewLine);
                await context.Response.Body.FlushAsync();

                var appDbContext = 
                    context.RequestServices.GetRequiredService<AppDbContext>();
                await appDbContext.Database.EnsureDeletedAsync();

                await context.Response.WriteAsync("Done!");
                await context.Response.WriteAsync(Environment.NewLine);
                await context.Response.WriteAsync("Navigate to /health to see the health status.");
                await context.Response.WriteAsync(Environment.NewLine);
            }));

            app.Run(async (context) =>
            {
                await context.Response.WriteAsync("Navigate to /health to see the health status.");
                await context.Response.WriteAsync(Environment.NewLine);
                await context.Response.WriteAsync("Navigate to /createdatabase to create the database.");
                await context.Response.WriteAsync(Environment.NewLine);
                await context.Response.WriteAsync("Navigate to /deletedatabase to delete the database.");
                await context.Response.WriteAsync(Environment.NewLine);
            });
        }
    }
}
