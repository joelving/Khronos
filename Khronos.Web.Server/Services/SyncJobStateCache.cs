using System;
using System.Collections.Concurrent;

namespace Khronos.Web.Server.Services
{
    public class SyncJobStateCache : ConcurrentDictionary<Guid, (bool, string)>
    {
    }
}
