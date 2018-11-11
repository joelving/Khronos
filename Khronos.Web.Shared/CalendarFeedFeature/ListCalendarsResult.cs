using System.Collections.Generic;

namespace Khronos.Web.Shared.CalendarFeedFeature
{
    public class ListCalendarsResult : ApiCommandResult
    {
        public IEnumerable<CalendarFeed> Calendars { get; set; }
    }
}
