using Khronos.Data;
using Khronos.Shared;
using Khronos.Web.Server.Hubs;
using Khronos.Web.Server.Services;
using Khronos.Web.Shared;
using Microsoft.AspNetCore.Blazor.Server;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;
using Polly;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Mime;

namespace Khronos.Web.Server
{
    public class Startup
    {
        public Startup(IHostingEnvironment environment, IConfiguration configuration)
        {
            Environment = environment;
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        public IHostingEnvironment Environment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContextPool<CalendarFeedDbContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"));
                options.EnableSensitiveDataLogging();
            });

            services.AddSingleton<IBackgroundQueue<SyncJob>>(new SyncJobQueue(50));
            services.AddTransient<IBackgroundJobProcessor<SyncJob>, SyncJobProcessor>();
            services.AddHostedService<BackgroundQueueService<SyncJob>>();
            services.AddSingleton<SyncJobStateCache>();
            services.AddSingleton<IClock>(SystemClock.Instance);
            services.AddHttpClient("RetryBacking")
                .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler())
                .AddTransientHttpErrorPolicy(builder => builder.WaitAndRetryAsync(new[]
                {
                    TimeSpan.FromSeconds(1),
                    TimeSpan.FromSeconds(3),
                    TimeSpan.FromSeconds(5)
                }));

            services.AddConnections();
            services.AddSignalR(options => {
                    options.KeepAliveInterval = TimeSpan.FromSeconds(5);
                    options.EnableDetailedErrors = true;
                })
                .AddJsonProtocol();

            services.AddResponseCompression(options =>
            {
                options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[]
                {
                    MediaTypeNames.Application.Octet,
                    WasmMediaTypeNames.Application.Wasm,
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, CalendarFeedDbContext dbContext)
        {
            dbContext.Database.Migrate();

            app.UseResponseCompression();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSignalR(route =>
            {
                route.MapHub<CalendarHub>($"/{nameof(ICalendarHub)}");
            });

            app.UseBlazor<Client.Startup>();
        }
    }
}
