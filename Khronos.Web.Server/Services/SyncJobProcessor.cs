using Khronos.Data;
using Khronos.iCal;
using Khronos.Shared;
using Khronos.Web.Server.Hubs;
using Khronos.Web.Shared;
using Khronos.Web.Shared.CalendarFeedFeature;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Khronos.Web.Server.Services
{
    public class SyncJobProcessor : IBackgroundJobProcessor<SyncJob>
    {
        private readonly CalendarFeedDbContext _dbContext;
        private readonly HttpClient _httpClient;
        private readonly SyncJobStateCache _progressCache;
        private readonly IHubContext<CalendarHub, ICalendarClient> _hubContext;
        private readonly IClock _clock;

        public SyncJobProcessor(
            CalendarFeedDbContext dbContext,
            IHttpClientFactory httpClientFactory,
            SyncJobStateCache progressCache,
            IHubContext<CalendarHub, ICalendarClient> hubContext,
            IClock clock
            )
        {
            _dbContext = dbContext;
            _httpClient = httpClientFactory.CreateClient("RetryBacking");
            _progressCache = progressCache;
            _hubContext = hubContext;
            _clock = clock;
        }

        public async Task ProcessJob((SyncJob job, Action callback) data, CancellationToken cancellationToken)
        {
            var (job, callback) = data;
            try
            {
                await SetProgress(job.Id, true, "Fetching iCal feed.", _hubContext);
                // Make sure to pass response stream off to pipe before buffering. Otherwise, we'd not see much benefit of using pipes.
                var response = await _httpClient.GetAsync(job.FeedUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    await SetProgress(job.Id, true, $"Failed to fecth iCal feed: {response.ReasonPhrase}.", _hubContext);
                    return;
                }

                await SetProgress(job.Id, true, "Parsing iCal feed.", _hubContext);
                var events = await UTF8Parser.ProcessFeed(await response.Content.ReadAsStreamAsync());

                if (cancellationToken.IsCancellationRequested)
                    return;

                await SetProgress(job.Id, true, "Saving snapshot.", _hubContext);
                // Save a new snapshot.
                var feedId = await _dbContext.CalendarFeeds.AsNoTracking()
                    .Where(f => f.Url == job.FeedUrl)
                    .Select(f => f.Id)
                    .FirstOrDefaultAsync();

                var snapshot = new Data.Models.CalendarSnapshot
                {
                    CalendarId = feedId,
                    FetchedOn = _clock.GetCurrentInstant()
                };
                _dbContext.CalendarSnapshots.Add(snapshot);
                await _dbContext.SaveChangesAsync();

                await _dbContext.CalendarEvents.AddRangeAsync(events.Select(e => Data.Models.CalendarEvent.FromEvent(e, snapshot)).ToList());
                await _dbContext.SaveChangesAsync();

                if (cancellationToken.IsCancellationRequested)
                    return;

                await SetProgress(job.Id, false, $"Done", _hubContext);
            }
            catch (Exception ex)
            {
                await SetProgress(job.Id, false, $"Failure!\n{ex}", _hubContext);
            }
            finally
            {
                // Release our queue semaphore allowing an additional item to be processed.
                callback();
            }
        }

        private async Task SetProgress(Guid jobId, bool running, string progress, IHubContext<CalendarHub, ICalendarClient> hubContext)
        {
            _progressCache.AddOrUpdate(jobId, (running, progress), (guid, prog) => (running, progress));
            await hubContext.Clients.Groups($"{nameof(SyncJob)}:{jobId}").SetProgress(new JobProgressResult { Success = true, JobId = jobId, Running = running, Progress = progress });
        }
    }
}
