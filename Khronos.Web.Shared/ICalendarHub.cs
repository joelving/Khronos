using Khronos.Web.Shared.CalendarFeedFeature;
using System.Threading.Tasks;

namespace Khronos.Web.Shared
{
    public interface ICalendarHub
    {
        Task ListCalendars();
        Task GetCalendar(GetCalendarCommand command);
        Task AddCalendar(AddCalendarCommand command);
        Task RefreshCalendar(RefreshCalendarCommand command);
    }
}
