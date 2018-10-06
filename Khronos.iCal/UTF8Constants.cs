using System;
using System.Collections.Generic;
using System.Text;

namespace Khronos.iCal
{
    public static class UTF8Constants
    {
        public static readonly ReadOnlyMemory<byte> BeginCalendar   = Encoding.UTF8.GetBytes("BEGIN:VCALENDAR\r\n").AsMemory();
        public static readonly ReadOnlyMemory<byte> EndCalendar     = Encoding.UTF8.GetBytes("END:VCALENDAR\r\n").AsMemory();
        public static readonly ReadOnlyMemory<byte> BeginEvent      = Encoding.UTF8.GetBytes("BEGIN:VEVENT\r\n").AsMemory();
        public static readonly ReadOnlyMemory<byte> EndEvent        = Encoding.UTF8.GetBytes("END:VEVENT\r\n").AsMemory();
        public static readonly ReadOnlyMemory<byte> Start           = Encoding.UTF8.GetBytes("DTSTART").AsMemory();
        public static readonly ReadOnlyMemory<byte> Duration        = Encoding.UTF8.GetBytes("DURATION").AsMemory();
        public static readonly ReadOnlyMemory<byte> End             = Encoding.UTF8.GetBytes("DTEND").AsMemory();
        public static readonly ReadOnlyMemory<byte> Attendee        = Encoding.UTF8.GetBytes("ATTENDEE").AsMemory();
        public static readonly ReadOnlyMemory<byte> LastModified    = Encoding.UTF8.GetBytes("LAST-MODIFIED").AsMemory();
        public static readonly ReadOnlyMemory<byte> Status          = Encoding.UTF8.GetBytes("STATUS").AsMemory();
        public static readonly ReadOnlyMemory<byte> Summary         = Encoding.UTF8.GetBytes("SUMMARY").AsMemory();
        public static readonly ReadOnlyMemory<byte> UId             = Encoding.UTF8.GetBytes("UID").AsMemory();
        public static readonly ReadOnlyMemory<byte> Organizer       = Encoding.UTF8.GetBytes("ORGANIZER").AsMemory();
        public static readonly ReadOnlyMemory<byte> NewLine         = Encoding.UTF8.GetBytes("\r\n").AsMemory();
        public static readonly byte                 Tab             = Encoding.UTF8.GetBytes("\t")[0];
        public static readonly byte                 Space           = Encoding.UTF8.GetBytes(" ")[0];
        public static readonly byte                 Colon           = Encoding.UTF8.GetBytes(":")[0];
        public static readonly byte                 Semicolon       = Encoding.UTF8.GetBytes(";")[0];
    }
}
