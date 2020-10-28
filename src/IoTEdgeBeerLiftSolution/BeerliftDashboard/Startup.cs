using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using IoTEdgeConversationDashboard.Data;
using BeerliftDashboard.Data;
using Microsoft.AspNetCore.Mvc;

namespace BeerliftDashboard
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            // Connecting to Azure SignalR
            var csSignalR = Configuration["Azure:SignalR:ConnectionString"];
            services.AddSignalR().AddAzureSignalR(csSignalR);

            services.AddRazorPages();
            services.AddServerSideBlazor();

            var csIoTHub = Configuration["Azure:IoTHub:ConnectionString"];
            var ioTHubServiceClientService = new IoTHubServiceClientService(csIoTHub);

            services.AddSingleton(ioTHubServiceClientService);

            var csSqlite = Configuration.GetConnectionString("sqlLite");

            var sqliteService = new SqliteService(csSqlite);

            services.AddSingleton(sqliteService);

            services.AddSingleton<TelemetryService>();

            services.AddScoped<SessionService>();

            // ApiController support
            services.AddMvc(options => options.EnableEndpointRouting = false).SetCompatibilityVersion(CompatibilityVersion.Version_3_0);
            services.AddControllers().AddNewtonsoftJson();
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

            // ApiController support
            app.UseMvcWithDefaultRoute();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}