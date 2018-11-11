using BlazorSignalR;
using Khronos.Web.Shared;
using Khronos.Web.Shared.CalendarFeedFeature;
using Microsoft.AspNetCore.Blazor.Components;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Khronos.Web.Client.Pages
{
    public class CalendarsComponent : BlazorComponent, ICalendarClient, IDisposable
    {
        readonly Dictionary<Guid, string> jobToCalendar = new Dictionary<Guid, string>();

        protected Dictionary<string, (bool, string)> jobProgress = new Dictionary<string, (bool, string)>();
        protected List<CalendarFeed> Calendars = new List<CalendarFeed>();
        protected List<string> ErrorMessages = new List<string>();
        protected string NewCalendarName;
        protected string NewCalendarUrl;


        public Task GetCalendars()
            => calendarHub.InvokeAsync(nameof(ICalendarHub.ListCalendars));

        public Task GetCalendar(string url)
            => calendarHub.InvokeAsync(nameof(ICalendarHub.GetCalendar), new GetCalendarCommand { Url = url });

        public Task AddCalendar()
            => calendarHub.InvokeAsync<AddCalendarResult>(nameof(ICalendarHub.AddCalendar), new AddCalendarCommand { Name = NewCalendarName, Url = NewCalendarUrl });

        public Task RefreshCalendar(string url)
            => calendarHub.InvokeAsync<RefreshCalendarResult>(nameof(ICalendarHub.RefreshCalendar), new RefreshCalendarCommand { Url = url });

        protected override async Task OnInitAsync()
        {
            await SetupSignalR();
            await GetCalendars();
        }
        
        private HubConnection calendarHub;
        private async Task SetupSignalR()
        {
            calendarHub = new HubConnectionBuilder()
                .WithUrlBlazor($"/{nameof(ICalendarHub)}")
                .Build();

            RegisterApiCall<ListCalendarsResult>(nameof(ICalendarClient.ReceiveCalendars), ReceiveCalendars);
            RegisterApiCall<GetCalendarResult>(nameof(ICalendarClient.ReceiveCalendar), ReceiveCalendar);
            RegisterApiCall<AddCalendarResult>(nameof(ICalendarClient.CalendarAdded), CalendarAdded);
            RegisterApiCall<RefreshCalendarResult>(nameof(ICalendarClient.CalendarRefreshing), CalendarRefreshing);
            RegisterApiCall<JobProgressResult>(nameof(ICalendarClient.SetProgress), SetProgress);

            try
            {
                await calendarHub.StartAsync();
            }
            catch (Exception ex)
            {
                ErrorMessages = new List<string> { ex.StackTrace };
                StateHasChanged();
            }
        }

        private void RegisterApiCall<T>(string methodName, Func<T, Task> callback) where T : ApiCommandResult
            => calendarHub.On(methodName, async (T result) => await HandleApiResult(result, callback));
        private async Task HandleApiResult<T>(T result, Func<T, Task> callback) where T : ApiCommandResult
        {
            if (!result.Success)
            {
                ErrorMessages = result.ErrorMessages;
                StateHasChanged();
                return;
            }

            await callback(result);
        }
        public async Task SetProgress(JobProgressResult result)
        {
            var url = jobToCalendar[result.JobId];
            jobProgress[url] = (result.Running, result.Progress);
            if (!result.Running)
                await GetCalendar(url);
            StateHasChanged();
        }
        public async Task ReceiveCalendars(ListCalendarsResult result)
        {
            Calendars = result.Calendars.ToList();
            StateHasChanged();
        }
        public async Task ReceiveCalendar(GetCalendarResult result)
        {
            Calendars.FirstOrDefault(c => c.Url == result.Calendar.Url).ReplaceWith(result.Calendar);
            StateHasChanged();
        }
        public async Task CalendarAdded(AddCalendarResult result)
        {
            Calendars.Add(new CalendarFeed
            {
                Name = result.Name,
                Url = result.Url
            });
            StateHasChanged();
        }
        public async Task CalendarRefreshing(RefreshCalendarResult result)
        {
            // Map job Id to url.
            jobToCalendar[result.JobId] = result.Url;
        }

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    calendarHub.DisposeAsync().Wait();
                }

                disposedValue = true;
            }
        }
        
        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
