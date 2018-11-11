using Khronos.Data.Models;
using Khronos.Shared;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Khronos.Data
{
    public class CalendarFeedDbContext : DbContext
    {
        //private readonly bool _shouldConfigure;
        //public CalendarFeedDbContext() : base()
        //{
        //    _shouldConfigure = true;
        //}

        //protected override void OnConfiguring(DbContextOptionsBuilder builder)
        //{
        //    if (_shouldConfigure)
        //        builder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=Khronos-1;Trusted_Connection=True;MultipleActiveResultSets=true");
        //}

        public CalendarFeedDbContext(DbContextOptions<CalendarFeedDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<CalendarSnapshot>()
                .Property(e => e.FetchedOn)
                .HasConversion(
                    v => v.ToUnixTimeMilliseconds(),
                    v => NodaTime.Instant.FromUnixTimeMilliseconds(v)
                );

            modelBuilder
                .Entity<CalendarEvent>()
                .Property(e => e.Start)
                .HasConversion(
                    v => v.ToUnixTimeMilliseconds(),
                    v => NodaTime.Instant.FromUnixTimeMilliseconds(v)
                );

            modelBuilder
                .Entity<CalendarEvent>()
                .Property(e => e.End)
                .HasConversion(
                    v => v.ToUnixTimeMilliseconds(),
                    v => NodaTime.Instant.FromUnixTimeMilliseconds(v)
                );

            modelBuilder
                .Entity<CalendarEvent>()
                .Property(e => e.Duration)
                .HasConversion(
                    v => v.TotalTicks,
                    v => NodaTime.Duration.FromTicks(v)
                );
        }

        public DbSet<CalendarFeed> CalendarFeeds { get; set; }
        public DbSet<CalendarSnapshot> CalendarSnapshots { get; set; }
        public DbSet<CalendarEvent> CalendarEvents { get; set; }
        public DbSet<SyncJob> PendingSyncJobs { get; set; }
    }
}
