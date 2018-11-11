using Khronos.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Khronos.Web.Server.Infrastructure
{
    public static class DbExtensions
    {
        public static async Task<IEnumerable<Shared.CalendarFeed>> GetListDTOs(this CalendarFeedDbContext context)
        {
            // Get a list of Ids and urls.
            var feeds = await context.CalendarFeeds.AsNoTracking().Select(f => new { f.Id, f.Name, f.Url }).ToListAsync();

            // Get latest snapshot
            var snapshots = await context.CalendarSnapshots.Include(s => s.Events).AsNoTracking()
                .GroupBy(s => s.CalendarId)
                .Select(s => new { CalendarId = s.Key, Latest = s.FirstOrDefault(sn => sn.FetchedOn == s.Max(cs => cs.FetchedOn)) })
                .ToListAsync();

            var snapshotIds = snapshots.Select(s => s.Latest.Id);
            // Get event counts for snapshots
            var eventCounts = await context.CalendarEvents.AsNoTracking()
                .Where(e => snapshotIds.Contains(e.SnapshotId))
                .GroupBy(e => e.SnapshotId)
                .Select(g => new { SnapshotId = g.Key, Count = g.Count() })
                .ToListAsync();

            var meta = snapshots.ToDictionary(s => s.CalendarId, s => new Shared.SnapshotMeta
            {
                FetchedOn = s.Latest.FetchedOn.ToDateTimeUtc(),
                NumberOfEvents = eventCounts.FirstOrDefault(e => e.SnapshotId == s.Latest.Id)?.Count ?? 0
            });

            return feeds.Select(f => new Shared.CalendarFeed
            {
                Name = f.Name,
                Url = f.Url,
                LatestSnapshot = meta.ContainsKey(f.Id) ? meta[f.Id] : null
            });
        }
        
        public static async Task<Shared.CalendarFeed> GetDTO(this CalendarFeedDbContext context, string url)
        {
            var feed = await context.CalendarFeeds.AsNoTracking().Select(f => new { f.Id, f.Name, f.Url }).FirstOrDefaultAsync(f => f.Url == url);

            // Get latest snapshot
            var snapshot = await context.CalendarSnapshots.Include(s => s.Events).AsNoTracking()
                .FirstOrDefaultAsync(s => s.CalendarId == feed.Id && s.FetchedOn == context.CalendarSnapshots.Max(cs => cs.FetchedOn));
            
            // Get event counts for snapshots
            var eventCount = await context.CalendarEvents.AsNoTracking()
                .CountAsync(e => e.SnapshotId == snapshot.Id);

            return new Shared.CalendarFeed
            {
                Name = feed.Name,
                Url = feed.Url,
                LatestSnapshot =new Shared.SnapshotMeta
                {
                    FetchedOn = snapshot.FetchedOn.ToDateTimeUtc(),
                    NumberOfEvents = eventCount
                }
            };
        }
    }
}
