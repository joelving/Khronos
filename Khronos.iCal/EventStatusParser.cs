using Khronos.Shared;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;

namespace Khronos.iCal
{
    public static class EventStatusParser
    {
        private static ReadOnlyMemory<byte> tenativeBytes = Encoding.UTF8.GetBytes("TENTATIVE").AsMemory();
        private static ReadOnlyMemory<byte> confirmedBytes = Encoding.UTF8.GetBytes("CONFIRMED").AsMemory();

        private static ReadOnlyMemory<byte> needsactionBytes = Encoding.UTF8.GetBytes("NEEDS-ACTION").AsMemory();
        private static ReadOnlyMemory<byte> completedBytes = Encoding.UTF8.GetBytes("COMPLETED").AsMemory();
        private static ReadOnlyMemory<byte> inprocessBytes = Encoding.UTF8.GetBytes("IN-PROCESS").AsMemory();

        private static ReadOnlyMemory<byte> draftBytes = Encoding.UTF8.GetBytes("DRAFT").AsMemory();
        private static ReadOnlyMemory<byte> finalBytes = Encoding.UTF8.GetBytes("FINAL").AsMemory();

        private static ReadOnlyMemory<byte> cancelledBytes = Encoding.UTF8.GetBytes("CANCELLED").AsMemory();

        public static EventStatus? Parse(ReadOnlySequence<byte> buffer)
        {
            if (buffer.MatchesFrom(tenativeBytes.Span)) return EventStatus.Tentative;
            if (buffer.MatchesFrom(confirmedBytes.Span)) return EventStatus.Confirmed;

            if (buffer.MatchesFrom(needsactionBytes.Span)) return EventStatus.NeedsAction;
            if (buffer.MatchesFrom(completedBytes.Span)) return EventStatus.Completed;
            if (buffer.MatchesFrom(inprocessBytes.Span)) return EventStatus.InProgress;

            if (buffer.MatchesFrom(draftBytes.Span)) return EventStatus.Draft;
            if (buffer.MatchesFrom(finalBytes.Span)) return EventStatus.Final;

            if (buffer.MatchesFrom(cancelledBytes.Span)) return EventStatus.Cancelled;

            return null;
        }
    }
}
