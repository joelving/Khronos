using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Khronos.Shared
{
    public class SyncJob
    {
        public Guid Id { get; set; }
        public string Owner { get; set; }
        public string FeedUrl { get; set; }
    }
}
