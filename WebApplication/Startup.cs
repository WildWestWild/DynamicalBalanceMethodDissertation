using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Primitives;
using SocketCreatingLib;
using WebApplication.ParserArea;

namespace WebApplication
{
    public class Startup
    {
        private const string StaticRequestWasFinished = "Последовательность запросов статического метода была обработана!";
        private const string DynamicRequestWasFinished = "Последовательность запросов динамического метода была обработана!";
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IUsableTable>(_ => new TableWeightHandler());
            services.AddRazorPages();
            services.Configure<KestrelServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;
            });

            // If using IIS:
            services.Configure<IISServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/RefreshDataDuration", async context =>
                {
                    ResetParams(context);
                    await context.Response.WriteAsync("Data duration has refreshed!");
                });
                endpoints.MapGet("/Low", async context =>
                {
                    await DynamicBalanceProcessingClientRequest(context);
                    await context.Response.WriteAsync(DynamicRequestWasFinished);
                });
                endpoints.MapGet("/Medium", async context =>
                {
                    await DynamicBalanceProcessingClientRequest(context);
                    await context.Response.WriteAsync(DynamicRequestWasFinished);
                });
                endpoints.MapGet("/High", async context =>
                {
                    await DynamicBalanceProcessingClientRequest(context);
                    await context.Response.WriteAsync(DynamicRequestWasFinished);
                });
                endpoints.MapGet("/LowStatic", async context =>
                {
                    await StaticBalanceProcessingClientRequest(context);
                    await context.Response.WriteAsync("Low has accepted!");
                });
                endpoints.MapGet("/MediumStatic", async context =>
                {
                    await StaticBalanceProcessingClientRequest(context);
                    await context.Response.WriteAsync(StaticRequestWasFinished);
                });
                endpoints.MapGet("/HighStatic", async context =>
                {
                    await StaticBalanceProcessingClientRequest(context);
                    await context.Response.WriteAsync(StaticRequestWasFinished);
                });
                endpoints.MapPost("/Server", async context =>
                {
                    ProcessingServerRequest(context);
                    await context.Response.WriteAsync(StaticRequestWasFinished);
                });
                endpoints.MapRazorPages();
            });
        }

        private void ResetParams(HttpContext context)
        {
            IUsableTable tableHandler = context.RequestServices.GetService<IUsableTable>();
            tableHandler.RemoveDurationInWeightTable();
            TableWeightHandler.ArrayOfRequests = new List<RequestTime>();
            CSVHandler.ResetRequestCounter();
            tableHandler.StartTimer();
        }

        private async Task DynamicBalanceProcessingClientRequest(HttpContext context)
        {
            var webApiCommandTime = context.Request.Query.Keys.FirstOrDefault();
            StringValues value;
            context.Request.Query.TryGetValue(webApiCommandTime, out value);
            if (!string.IsNullOrEmpty(value.FirstOrDefault()))
            {
                var webServerRequest = new WebServerRequest
                {
                    WebApiCommandTime = value.FirstOrDefault(),
                    IpAddress = String.Empty
                };
                IUsableTable tableHandler = context.RequestServices.GetService<IUsableTable>();
                CSVHandler.UseCSVHandler(tableHandler);
                Console.WriteLine($"{context.Request.QueryString} + get started with WebApiCommand = {value}");
                await tableHandler.DynamicSendRequestByTableWeight(webServerRequest);
            }
        }
        
        private async Task StaticBalanceProcessingClientRequest(HttpContext context)
        {
            var webApiCommandTime = context.Request.Query.Keys.FirstOrDefault();
            StringValues value;
            context.Request.Query.TryGetValue(webApiCommandTime, out value);
            if (!string.IsNullOrEmpty(value.FirstOrDefault()))
            {
                var webServerRequest = new WebServerRequest
                {
                    WebApiCommandTime = value.FirstOrDefault(),
                    IpAddress = String.Empty
                };
                IUsableTable tableHandler = context.RequestServices.GetService<IUsableTable>();
                Console.WriteLine($"{context.Request.QueryString} + get started STATIC with WebApiCommand = {value}");
                await tableHandler.StaticSendRequestByTableWeight(webServerRequest);
            }
        }

        private void ProcessingServerRequest(HttpContext context)
        {
            if (context != null)
            {
                var webServerRequest = WebServerRequest.Deserialize(context.Request.Body.GetStringByStream());
                IUsableTable tableHandler = context.RequestServices.GetService<IUsableTable>();
                tableHandler.UseTable(webServerRequest);
                Console.WriteLine($"Connection by Port-{webServerRequest.Port} was created");
            }
        }
    }
}