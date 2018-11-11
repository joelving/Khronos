using System;
using System.Collections.Generic;
using System.Text;

namespace Khronos.Web.Shared.CalendarFeedFeature
{
    public class RefreshCalendarResult : ApiCommandResult
    {
        public Guid JobId { get; set; }
        public string Url { get; set; }
    }
}
