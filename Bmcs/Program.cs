using Bmcs.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
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

            // Add services to the container (旧ConfigureServices部分)

            //セッションの保存先
            //※メモリ保持だとアプリの再起動（デプロイ・スケール・プラットフォーム保守）で
            //  全員がログアウトし、スコア入力中のユーザが弾き出される。
            //  接続文字列がある場合はSQL Serverに保持する。
            var sessionConnectionString = builder.Configuration.GetConnectionString("SqlServerConnectionString");

            if (string.IsNullOrEmpty(sessionConnectionString))
            {
                //接続できない環境（設定漏れ）でも起動はできるようにする
                builder.Services.AddDistributedMemoryCache();
            }
            else
            {
                builder.Services.AddDistributedSqlServerCache(options =>
                {
                    options.ConnectionString = sessionConnectionString;
                    options.SchemaName = "dbo";
                    options.TableName = "SessionCache";
                    //期限切れレコードの削除間隔
                    options.ExpiredItemsDeletionInterval = TimeSpan.FromMinutes(30);
                });
            }

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

            // Email Service Registration
            builder.Services.AddTransient<Bmcs.Function.IEmailSender, Bmcs.Function.EmailSender>();

            // 試行回数制限（アカウント復旧機能の総当たり対策）
            builder.Services.AddMemoryCache();
            builder.Services.AddSingleton<Bmcs.Function.IRateLimiter, Bmcs.Function.RateLimiter>();

            // Azure App Service等のリバースプロキシ配下で、アクセス元IPを正しく取得する
            // ※未設定の場合、全リクエストが同一IPと判定され、試行回数制限がサイト全体で共有されてしまう
            builder.Services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                //Azure App Serviceのフロントエンドを信頼するため、既定の制限を解除する
                options.KnownNetworks.Clear();
                options.KnownProxies.Clear();
            });

            var app = builder.Build();

            //プロキシからのヘッダを反映する（他のミドルウェアより先に実行する）
            app.UseForwardedHeaders();

            // Configure the HTTP request pipeline (旧Configure部分)
            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            // DBの初期化（旧CreateDbIfNotExistsの内容）
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