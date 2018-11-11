using System;

namespace Khronos.Web.Shared.CalendarFeedFeature
{
    public class JobProgressResult : ApiCommandResult
    {
        public Guid JobId { get; set; }
        public bool Running { get; set; }
        public string Progress { get; set; }
    }
}
