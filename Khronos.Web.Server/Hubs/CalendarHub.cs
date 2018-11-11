using Khronos.Shared;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Khronos.Web.Shared;
using Khronos.Web.Server.Services;
using Khronos.Web.Shared.CalendarFeedFeature;
using Khronos.Data;
using Microsoft.EntityFrameworkCore;
using Khronos.Web.Server.Infrastructure;
using System.Collections.Generic;

namespace Khronos.Web.Server.Hubs
{
    public class CalendarHub : Hub<ICalendarClient>, ICalendarHub
    {
        private readonly CalendarFeedDbContext _dbContext;
        private readonly ISyncJobQueue _syncJobQueue;
        private readonly SyncJobStateCache _progressCache;

        public CalendarHub(CalendarFeedDbContext dbContext, ISyncJobQueue syncJobQueue, SyncJobStateCache progressCache)
        {
            _dbContext = dbContext;
            _syncJobQueue = syncJobQueue;
            _progressCache = progressCache;
        }

        public async Task ListCalendars()
            => await Clients.Caller.ReceiveCalendars(new ListCalendarsResult
            {
                Success = true,
                Calendars = await _dbContext.GetListDTOs()
            });

        public async Task GetCalendar(GetCalendarCommand command)
            => await Clients.Caller.ReceiveCalendar(new GetCalendarResult
            {
                Success = true,
                Calendar = await _dbContext.GetDTO(command.Url)
            });

        public async Task AddCalendar(AddCalendarCommand command)
        {
            var exists = await _dbContext.CalendarFeeds.AnyAsync(c => c.Url == command.Url);
            if (exists)
            {
                await Clients.Caller.CalendarAdded(new AddCalendarResult { ErrorMessages = new List<string> { $"The calendar with the url {command.Url} is already registered." } });
                return;
            }

            var cal = new Data.Models.CalendarFeed { Name = command.Name, Url = command.Url };
            await _dbContext.CalendarFeeds.AddAsync(cal);
            await _dbContext.SaveChangesAsync();

            await Clients.Caller.CalendarAdded(new AddCalendarResult { Success = true, Name = command.Name, Url = command.Url });
        }

        public async Task RefreshCalendar(RefreshCalendarCommand command)
        {
            var exists = await _dbContext.CalendarFeeds.AnyAsync(c => c.Url == command.Url);
            if (!exists)
            {
                await Clients.Caller.CalendarRefreshing(new RefreshCalendarResult { ErrorMessages = new List<string> { "The calendar isn't registered." } });
                return;
            }

            var job = new SyncJob
            {
                Id = Guid.NewGuid(),
                FeedUrl = command.Url,
                Owner = Context.User.Identity?.Name
            };

            _syncJobQueue.QueueSyncJob(job);

            await Clients.Caller.CalendarRefreshing(new RefreshCalendarResult
            {
                Success = true,
                JobId = job.Id,
                Url = command.Url
            });

            await SubscribeToJob(job.Id);
        }
        
        public async Task SubscribeToJob(Guid jobId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"{nameof(SyncJob)}:{jobId}");
            if (_progressCache.ContainsKey(jobId))
            {
                var (running, progress) = _progressCache[jobId];
                await Clients.Caller.SetProgress(new JobProgressResult { Success = true, JobId = jobId, Running = running, Progress = progress });
            }
        }

        public async Task UnsubscribeFromJob(Guid jobId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"{nameof(SyncJob)}:{jobId}");
        }
    }
}
