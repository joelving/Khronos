using Khronos.Web.Shared.CalendarFeedFeature;
using System;
using System.Threading.Tasks;

namespace Khronos.Web.Shared
{
    public interface ICalendarClient
    {
        Task ReceiveCalendars(ListCalendarsResult result);
        Task ReceiveCalendar(GetCalendarResult result);
        Task CalendarAdded(AddCalendarResult result);
        Task CalendarRefreshing(RefreshCalendarResult result);
        Task SetProgress(JobProgressResult result);
    }
}
