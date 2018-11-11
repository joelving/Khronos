using NodaTime;
using System;

namespace Khronos.Web.Shared
{
    public class SnapshotMeta
    {
        public DateTime FetchedOn { get; set; }

        public int NumberOfEvents { get; set; }

        internal void ReplaceWith(SnapshotMeta latestSnapshot)
        {
            FetchedOn = latestSnapshot.FetchedOn;
            NumberOfEvents = latestSnapshot.NumberOfEvents;
        }
    }
}
