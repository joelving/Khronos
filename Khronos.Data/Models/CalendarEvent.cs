using Khronos.Shared;
using NodaTime;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Khronos.Data.Models
{
    public class CalendarEvent
    {
        public long Id { get; set; }
        public long SnapshotId { get; set; }
        public virtual CalendarSnapshot Snapshot { get; set; }

        public string UId { get; set; }
        public Instant Start { get; set; }
        public Instant End { get; set; }
        public Duration Duration { get; set; }
        public EventStatus? Status { get; set; }
        public string Summary { get; set; }

        /// <summary>
        /// DON'T USE!
        /// </summary>
        public string _attendees { get; set; }
        [NotMapped]
        public List<string> Attendees { get => _attendees.Split(new[] { "#|#" }, StringSplitOptions.RemoveEmptyEntries).ToList(); set => _attendees = string.Join("#|#", value); }

        public static CalendarEvent FromEvent(Event @event)
            => new CalendarEvent
            {
                UId = @event.UId,
                Start = @event.Start,
                End = @event.End,
                Duration = @event.Duration,
                Status = @event.Status,
                Summary = @event.Summary,
                Attendees = @event.Attendees.Select(a => a).ToList()
            };
    }
}