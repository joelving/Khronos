using NodaTime;
using System.Collections.Generic;

namespace Khronos.Data.Models
{
    public class CalendarSnapshot
    {
        public long Id { get; set; }

        public long CalendarId { get; set; }
        public virtual CalendarFeed Calendar { get; set; }

        public Instant FetchedOn { get; set; }

        public List<CalendarEvent> Events { get; set; }
    }
}