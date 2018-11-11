using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Khronos.Data.Models
{
    public class CalendarFeed
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public string Url { get; set; }

        public List<CalendarSnapshot> Snapshots { get; set; }
    }
}
