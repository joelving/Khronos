﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Khronos.Data.Migrations
{
    [DbContext(typeof(CalendarFeedDbContext))]
    [Migration("20181109073824_CalendarShortname")]
    partial class CalendarShortname
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.1.4-rtm-31024")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Khronos.Web.Server.Data.Models.CalendarEvent", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<double>("Duration");

                    b.Property<long>("End");

                    b.Property<long>("SnapshotId");

                    b.Property<long>("Start");

                    b.Property<int?>("Status");

                    b.Property<string>("Summary");

                    b.Property<string>("UId");

                    b.Property<string>("_attendees");

                    b.HasKey("Id");

                    b.HasIndex("SnapshotId");

                    b.ToTable("CalendarEvent");
                });

            modelBuilder.Entity("Khronos.Web.Server.Data.Models.CalendarFeed", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Name");

                    b.Property<string>("Url");

                    b.HasKey("Id");

                    b.ToTable("CalendarFeeds");
                });

            modelBuilder.Entity("Khronos.Web.Server.Data.Models.CalendarSnapshot", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<long>("CalendarId");

                    b.Property<long>("FetchedOn");

                    b.HasKey("Id");

                    b.HasIndex("CalendarId");

                    b.ToTable("CalendarSnapshots");
                });

            modelBuilder.Entity("Khronos.Web.Server.Models.SyncJob", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("FeedUrl");

                    b.Property<string>("Owner");

                    b.HasKey("Id");

                    b.ToTable("PendingSyncJobs");
                });

            modelBuilder.Entity("Khronos.Web.Server.Data.Models.CalendarEvent", b =>
                {
                    b.HasOne("Khronos.Web.Server.Data.Models.CalendarSnapshot", "Snapshot")
                        .WithMany("Events")
                        .HasForeignKey("SnapshotId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Khronos.Web.Server.Data.Models.CalendarSnapshot", b =>
                {
                    b.HasOne("Khronos.Web.Server.Data.Models.CalendarFeed", "Calendar")
                        .WithMany("Snapshots")
                        .HasForeignKey("CalendarId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
