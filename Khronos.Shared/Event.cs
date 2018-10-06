using NodaTime;
using System.Collections.Generic;

namespace Khronos.Shared
{
    public class Event
    {
        public string UId { get; set; }
        public Instant Start { get; set; }
        public Instant End { get; set; }
        public Duration Duration { get; set; }
        public EventStatus? Status { get; set; }
        public string Summary { get; set; }

        public List<string> Attendees { get; set; } = new List<string>();
    }
}
