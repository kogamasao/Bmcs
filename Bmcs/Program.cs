using Bmcs.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;

namespace Bmcs
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container (ãåConfigureServicesïîï™)
            builder.Services.AddDistributedMemoryCache();

            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromHours(1);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            builder.Services.AddRazorPages()
                .AddRazorPagesOptions(options =>
                {
                    //options.Conventions.AddPageRoute("/Login/Index", "");
                });

            if (builder.Environment.IsDevelopment())
            {
                builder.Services.AddDbContext<BmcsContext>(options =>
                    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServerConnectionString")));
            }
            else
            {
                builder.Services.AddDbContext<BmcsContext>(options =>
                    options.UseSqlServer(builder.Configuration.GetConnectionString("AzureDatabaseConnectionString")));
            }

            var app = builder.Build();

            // Configure the HTTP request pipeline (ãåConfigureïîï™)
            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            // DBÇÃèâä˙âªÅiãåCreateDbIfNotExistsÇÃì‡óeÅj
            CreateDbIfNotExists(app);

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();
            app.UseSession();

            app.MapRazorPages();

            app.Run();
        }

        private static void CreateDbIfNotExists(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var services = scope.ServiceProvider;
            try
            {
                var context = services.GetRequiredService<BmcsContext>();
                DbInitializer.Initialize(context);
            }
            catch (Exception ex)
            {
                var logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "An error occurred creating the DB.");
            }
        }
    }
}