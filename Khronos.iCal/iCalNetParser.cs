using Ical.Net;
using Khronos.Shared;
using NodaTime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Khronos.iCal
{
    public static class iCalNetParser
    {

        public static List<Event> LoadAndParseFeed(FileInfo file)
        {
            using (var reader = file.OpenRead())
            {
                return Calendar.Load(reader).Events.Select(e => new Event
                {
                    UId = e.Uid,
                    Attendees = e.Attendees.Select(a => a.Value.ToString()).ToList(),
                    Start = e.Start == null ? default : Instant.FromDateTimeUtc(e.Start.Value.ToUniversalTime()),
                    End = e.End == null ? default : Instant.FromDateTimeUtc(e.End.Value.ToUniversalTime()),
                    Duration = e.Duration == null ? default : Duration.FromTimeSpan(e.Duration),
                    Summary = e.Summary
                }).ToList();
            }
        }
    }
}
