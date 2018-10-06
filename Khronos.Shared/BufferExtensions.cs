using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Text;

namespace Khronos.Shared
{
    public static class BufferExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToString(in this ReadOnlySequence<byte> buffer, Encoding encoding)
        {
            if (buffer.IsSingleSegment)
            {
                return encoding.GetString(buffer.First.Span);
            }

            return string.Create((int)buffer.Length, buffer, (span, sequence) =>
            {
                foreach (var segment in sequence)
                {
                    encoding.GetChars(segment.Span, span);
                    span = span.Slice(segment.Length);
                }
            });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SequencePosition? PositionOf<T>(in this ReadOnlySequence<T> source, ReadOnlySpan<T> value) where T : IEquatable<T>
        {
            if (source.IsEmpty || value.IsEmpty)
                return null;

            if (source.IsSingleSegment)
            {
                var index = source.First.Span.IndexOf(value);
                if (index > -1)
                    return source.GetPosition(index);
                else
                    return null;
            }

            return PositionOfMultiSegment(source, value);
        }

        public static SequencePosition? PositionOfMultiSegment<T>(in ReadOnlySequence<T> source, ReadOnlySpan<T> value) where T : IEquatable<T>
        {
            var firstVal = value[0];

            SequencePosition position = source.Start;
            SequencePosition result = position;
            while (source.TryGet(ref position, out ReadOnlyMemory<T> memory))
            {
                var offset = 0;
                while (offset < memory.Length)
                {
                    var index = memory.Span.Slice(offset).IndexOf(firstVal);
                    if (index == -1)
                        break;

                    var candidatePos = source.GetPosition(index + offset, result);
                    if (source.MatchesFrom(value, candidatePos))
                        return candidatePos;

                    offset += index + 1;
                }
                if (position.GetObject() == null)
                {
                    break;
                }

                result = position;
            }

            return null;
        }


        public static bool MatchesFrom<T>(in this ReadOnlySequence<T> source, ReadOnlySpan<T> value, SequencePosition? position = null) where T : IEquatable<T>
        {
            var candidate = position == null ? source : source.Slice(position.Value, value.Length);
            if (candidate.Length != value.Length)
                return false;

            int i = 0;
            foreach (var sequence in candidate)
            {
                foreach (var entry in sequence.Span)
                {
                    if (!entry.Equals(value[i++]))
                        return false;
                }
            }
            return true;
        }
    }
}
