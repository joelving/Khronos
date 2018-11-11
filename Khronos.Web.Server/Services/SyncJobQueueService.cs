using Khronos.Data;
using Khronos.iCal;
using Khronos.Shared;
using Khronos.Web.Server.Hubs;
using Khronos.Web.Shared;
using Khronos.Web.Shared.CalendarFeedFeature;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NodaTime;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Khronos.Web.Server.Services
{
    public class SyncJobQueueService : BackgroundService
    {
        public ISyncJobQueue TaskQueue { get; }

        private readonly SyncJobStateCache _progressCache;
        private readonly ILogger _logger;
        private readonly HttpClient _httpClient;
        private readonly IClock _clock;
        private readonly IServiceProvider _services;

        public SyncJobQueueService(ISyncJobQueue taskQueue, SyncJobStateCache progressCache, IHttpClientFactory httpClientFactory, IClock clock, ILoggerFactory loggerFactory, IServiceProvider services)
        {
            TaskQueue = taskQueue;
            _progressCache = progressCache;
            _httpClient = httpClientFactory.CreateClient("RetryBacking");
            _clock = clock;
            _logger = loggerFactory.CreateLogger<SyncJobQueueService>();
            _services = services;
        }

        protected async override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("SyncJob queue Service is starting.");
            
            while (!cancellationToken.IsCancellationRequested)
            {
                var item = await TaskQueue.DequeueAsync(cancellationToken);

                Task.Run(async () =>
                {
                    if (cancellationToken.IsCancellationRequested)
                        return;

                    using (var scope = _services.CreateScope()) {
                        var dbContext = scope.ServiceProvider.GetRequiredService<CalendarFeedDbContext>();
                        var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<CalendarHub, ICalendarClient>>();

                        // The queue is running on it's own thread, dispatching jobs to the thread pool. This is fine since the processing is async and non-blocking.
                        await ProcessSyncJob(item, dbContext, hubContext, _clock, cancellationToken);
                    }
                }, cancellationToken);
            }

            _logger.LogInformation("SyncJob queue Service is stopping.");
        }
        
        private async Task ProcessSyncJob(SyncJob job, CalendarFeedDbContext dbContext, IHubContext<CalendarHub, ICalendarClient> hubContext, IClock _clock, CancellationToken cancellationToken)
        {
            try
            {
                await SetProgress(job.Id, true, "Fetching iCal feed.", hubContext);
                // Make sure to pass response stream off to pipe before buffering. Otherwise, we'd not see much benefit of using pipes.
                var response = await _httpClient.GetAsync(job.FeedUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                await SetProgress(job.Id, true, "Parsing iCal feed.", hubContext);
                var events = await UTF8Parser.ProcessFeed(await response.Content.ReadAsStreamAsync());

                if (cancellationToken.IsCancellationRequested)
                    return;
                
                await SetProgress(job.Id, true, "Saving snapshot.", hubContext);
                // Save a new snapshot.
                var ev = await dbContext.CalendarFeeds
                    .Include(f => f.Snapshots)
                    .ThenInclude(s => s.Events)
                    .FirstOrDefaultAsync(c => c.Url == job.FeedUrl);

                ev.Snapshots.Add(new Data.Models.CalendarSnapshot
                {
                    FetchedOn = _clock.GetCurrentInstant(),
                    Events = events.Select(e => Data.Models.CalendarEvent.FromEvent(e)).ToList()
                });

                await dbContext.SaveChangesAsync();

                if (cancellationToken.IsCancellationRequested)
                    return;

                // TODO: Signal completion
                await SetProgress(job.Id, false, $"Done", hubContext);
            }
            catch (Exception ex)
            {
                await SetProgress(job.Id, false, $"Failure!\n{ex}", hubContext);
                // TODO: Signal failure
            }
        }

        private async Task SetProgress(Guid jobId, bool running, string progress, IHubContext<CalendarHub, ICalendarClient> hubContext)
        {
            _progressCache.AddOrUpdate(jobId, (running, progress), (guid, prog) => (running, progress));
            await hubContext.Clients.Groups($"{nameof(SyncJob)}:{jobId}").SetProgress(new JobProgressResult { Success = true, JobId = jobId, Running = running, Progress = progress });
        }
    }
}
