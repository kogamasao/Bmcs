using Bmcs.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
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

            //接続文字列
            //※環境によってキーが異なる。EFとセッションで別々に取得すると、
            //  片方だけ設定漏れになっても気づけないため、ここで1つに決める。
            var connectionString = builder.Environment.IsDevelopment()
                                   ? builder.Configuration.GetConnectionString("SqlServerConnectionString")
                                   : builder.Configuration.GetConnectionString("AzureDatabaseConnectionString");

            //セッションの保存先
            //※メモリ保持だとアプリの再起動（デプロイ・スケール・プラットフォーム保守）で
            //  全員がログアウトし、スコア入力中のユーザが弾き出される。
            var isSessionInMemory = string.IsNullOrEmpty(connectionString);

            if (isSessionInMemory)
            {
                //接続文字列が無い環境でも起動はできるようにする（起動後に警告を出す）
                builder.Services.AddDistributedMemoryCache();
            }
            else
            {
                builder.Services.AddDistributedSqlServerCache(options =>
                {
                    options.ConnectionString = connectionString;
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

            builder.Services.AddDbContext<BmcsContext>(options =>
                options.UseSqlServer(connectionString));

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
                options.KnownIPNetworks.Clear();
                options.KnownProxies.Clear();
            });

            var app = builder.Build();

            //プロキシからのヘッダを反映する（他のミドルウェアより先に実行する）
            app.UseForwardedHeaders();

            //正規のホスト名（独自ドメイン）への転送
            //※旧URL（bmcs.azurewebsites.net）や App Service の既定のホスト名で来たアクセスを、独自ドメインへ恒久的に転送する。
            //  検索エンジンの評価と、共有済みのリンクを引き継ぐため。設定（Site:CanonicalHost）が無い環境（ローカル等）では何もしない
            var canonicalHost = builder.Configuration["Site:CanonicalHost"];

            if (!string.IsNullOrWhiteSpace(canonicalHost))
            {
                app.Use(async (context, next) =>
                {
                    var host = context.Request.Host.Host;

                    if (!string.Equals(host, canonicalHost, StringComparison.OrdinalIgnoreCase)
                        && !string.Equals(host, "localhost", StringComparison.OrdinalIgnoreCase))
                    {
                        var url = "https://" + canonicalHost + context.Request.PathBase + context.Request.Path + context.Request.QueryString;

                        //GET・HEAD は 301、それ以外（フォームの送信など）は本文を保ったまま転送できる 308 にする
                        context.Response.StatusCode = HttpMethods.IsGet(context.Request.Method) || HttpMethods.IsHead(context.Request.Method)
                                                      ? StatusCodes.Status301MovedPermanently
                                                      : StatusCodes.Status308PermanentRedirect;
                        context.Response.Headers.Location = url;

                        return;
                    }

                    await next();
                });
            }

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

            if (isSessionInMemory)
            {
                //無言でメモリ保持になると「直したつもりで直っていない」状態になるため警告する
                app.Logger.LogWarning("接続文字列が取得できないため、セッションをメモリに保持します。アプリの再起動で全員がログアウトします。");
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