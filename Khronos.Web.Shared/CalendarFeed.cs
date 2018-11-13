using System;
using System.Collections.Generic;
using System.Text;

namespace Khronos.Web.Shared
{
    public class CalendarFeed
    {
        public string Name { get; set; }

        public string Url { get; set; }

        public SnapshotMeta LatestSnapshot { get; set; }

        public void ReplaceWith(CalendarFeed calendar)
        {
            Name = calendar.Name;
            Url = calendar.Url;
            if (LatestSnapshot is null)
                LatestSnapshot = calendar.LatestSnapshot;
            else
                LatestSnapshot.ReplaceWith(calendar.LatestSnapshot);
        }
    }
}
