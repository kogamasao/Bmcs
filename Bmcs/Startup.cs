using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Bmcs.Data;

namespace Bmcs
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            Env = env;
        }

        public IConfiguration Configuration { get; }
        public IWebHostEnvironment Env { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDistributedMemoryCache();

            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromHours(1);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            services.AddRazorPages()
                .AddRazorPagesOptions(options =>
                {
                    //options.Conventions.AddPageRoute("/Login/Index", "");
                });

            if (Env.IsDevelopment())
            {
                services.AddDbContext<BmcsContext>(options =>
                    options.UseSqlServer(Configuration.GetConnectionString("SqlServerConnectionString")));

                //services.AddDbContext<BmcsContext>(options =>
                //  options.UseCosmos(Configuration.GetConnectionString("CosmosAccountEndpoint")
                //      , Configuration.GetConnectionString("CosmosAccountKey")
                //      , Configuration.GetConnectionString("CosmosDatabaseName")));
            }
            else
            {
                services.AddDbContext<BmcsContext>(options =>
                 options.UseSqlServer(Configuration.GetConnectionString("AzureDatabaseConnectionString")));

                //services.AddDbContext<BmcsContext>(options =>
                //    options.UseCosmos(Configuration.GetConnectionString("CosmosAccountEndpoint")
                //        , Configuration.GetConnectionString("CosmosAccountKey")
                //        , Configuration.GetConnectionString("CosmosDatabaseName")));
            }

            //services.AddAntiforgery(options =>
            //{
            //    options.FormFieldName = "__RequestVerificationToken";
            //});
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
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();
            app.UseSession();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
            });
        }
    }
}
