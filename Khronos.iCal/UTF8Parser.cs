using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.IO.Pipelines;
using Pipelines.Sockets.Unofficial;
using System.Threading;
using System.Buffers;
using Khronos.Shared;
using NodaTime;
using NodaTime.Text;
using System.IO;

namespace Khronos.iCal
{
    public static class UTF8Parser
    {
        public static async Task<List<Event>> FetchAndParseFeed(string url)
        {
            HttpClient client = new HttpClient();
            var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            return await ProcessFeed(await response.Content.ReadAsStreamAsync());
        }

        public static async Task<List<Event>> LoadAndParseFeed(FileInfo file)
        {
            using (var reader = file.OpenRead())
            {
                return await ProcessFeed(reader);
            }
        }

        public static async Task<List<Event>> ProcessFeed(Stream stream)
        {
            var reader = StreamConnection.GetReader(stream);

            var events = new List<Event>();
            void catchEvent (Event ev) => events.Add(ev);
            await GetEvents(reader, catchEvent);
            return events;
        }

        private static void CatchEvent(Event ev, List<Event> list)
            => list.Add(ev);

        public static async ValueTask GetEvents(PipeReader reader, Action<Event> callback, CancellationToken cancellationToken = default)
        {
            while (true)
            {
                // Tell the pipe we need more data.
                // It will only return once it has new data we haven't seen.
                // Unlike a stream the pipe will return bytes we've previously seen but not consumed.
                var read = await reader.ReadAsync(cancellationToken);
                if (read.IsCanceled) return;
                var buffer = read.Buffer;

                if (TryParseEvent(buffer, out Event nextEvent, out SequencePosition consumedTo))
                {
                    // Like a stream, we can tell the pipe that we've used a certain amount of bytes.
                    // The pipe will then release that memory back the pool from which it was rented.
                    reader.AdvanceTo(consumedTo);
                    callback(nextEvent);
                    continue;
                }

                // We didn't find what we're looking for.
                // Signal the pipe that we've seen it all but haven't used anything.
                reader.AdvanceTo(consumedTo, buffer.End);
                if (read.IsCompleted) return;
            }
        }

        private static bool TryParseEvent(ReadOnlySequence<byte> buffer, out Event nextEvent, out SequencePosition consumedTo)
        {
            // Find the start of the event
            var begin = buffer.PositionOf(UTF8Constants.BeginEvent.Span);
            if (begin == null)
            {
                nextEvent = default;
                consumedTo = buffer.Start;
                return false;
            }
            // We found a beginning. Throw away data up to that point.
            consumedTo = begin.Value;

            // Can we also find an end to it?
            var end = buffer.PositionOf(UTF8Constants.EndEvent.Span);
            if (end == null)
            {
                nextEvent = default;
                consumedTo = default;
                return false;
            }

            var payload = buffer.Slice(buffer.GetPosition(UTF8Constants.BeginEvent.Length, begin.Value), end.Value);
            ReadEvent(payload, out nextEvent);
            consumedTo = buffer.GetPosition(UTF8Constants.EndEvent.Length, end.Value);

            return true;
        }

        private static void ReadEvent(ReadOnlySequence<byte> payload, out Event nextEvent)
        {
            nextEvent = new Event();
            // Loop through all the content lines and parse them.
            var eof = false;
            var linestart = 0L;
            while (!eof)
            {
                // Find the next line
                var offset = linestart;
                SequencePosition? eol = null;
                while (offset < payload.Length)
                {
                    var remainder = payload.Slice(offset);
                    offset = payload.Length - remainder.Length;
                    eol = remainder.PositionOf(UTF8Constants.NewLine.Span);
                    if (eol == null)
                    {
                        // We're past the last line break and thus done!
                        eof = true;
                        return;
                    }

                    // We got a CRLF - check that it's not followed by a tab or a space.
                    var atCRLF = remainder.Slice(eol.Value);
                    if (atCRLF.Length > UTF8Constants.NewLine.Length)
                    {
                        var nextByte = atCRLF.Slice(UTF8Constants.NewLine.Length, 1).First.Span[0];
                        if (nextByte == UTF8Constants.Tab || nextByte == UTF8Constants.Space)
                        {
                            offset += payload.Length - atCRLF.Length + UTF8Constants.NewLine.Length + 1;
                            continue;
                        }
                    }

                    // Slice from start to line break
                    var line = payload.Slice(linestart, eol.Value);
                    TryParseLine(line, nextEvent);
                    // Read past the line break
                    linestart += line.Length + UTF8Constants.NewLine.Length;
                    break;
                }
                if (offset >= payload.Length)
                    break;
            }
        }

        private static bool TryParseLine(ReadOnlySequence<byte> buffer, Event nextEvent)
        {
            // Per RFC 5545 contentlines have the following syntax:
            // contentline = name *(";" param ) ":" value CRLF
            // meaning we'll read until ; or : and treat accordingly

            var valueDelim = buffer.PositionOf(UTF8Constants.Colon);
            if (valueDelim == null)
            {
                // The line is somehow invalid. Abort.
                return false;
            }
            
            var nameAndParams = buffer.Slice(0, valueDelim.Value);
            var value = buffer.Slice(valueDelim.Value).Slice(1);

            // Check for parameters - for our use, we don't care about their values, so we simply ignore them.
            var paramDelim = nameAndParams.PositionOf(UTF8Constants.Semicolon);
            var name = paramDelim == null ? nameAndParams : nameAndParams.Slice(0, paramDelim.Value);
            var parameters = paramDelim == null ? new ReadOnlySequence<byte>() : nameAndParams.Slice(paramDelim.Value).Slice(1);

            UpdateProperty(name, parameters, value, nextEvent);

            return true;
        }

        private static readonly InstantPattern iCalInstantPattern = InstantPattern.CreateWithInvariantCulture("uuuuMMdd'T'HHmmss'Z'");
        private static void UpdateProperty(ReadOnlySequence<byte> name, ReadOnlySequence<byte> parameters, ReadOnlySequence<byte> value, Event nextEvent)
        {
            if (name.MatchesFrom(UTF8Constants.Attendee.Span))
            {
                nextEvent.Attendees.Add(value.ToString(Encoding.UTF8));
            }
            else if (name.MatchesFrom(UTF8Constants.UId.Span))
            {
                nextEvent.UId = value.ToString(Encoding.UTF8);
            }
            else if (name.MatchesFrom(UTF8Constants.Start.Span))
            {
                var parseResult = iCalInstantPattern.Parse(value.ToString(Encoding.UTF8));
                if (parseResult.Success)
                    nextEvent.Start = parseResult.Value;
            }
            else if (name.MatchesFrom(UTF8Constants.End.Span))
            {
                var parseResult = iCalInstantPattern.Parse(value.ToString(Encoding.UTF8));
                if (parseResult.Success)
                    nextEvent.End = parseResult.Value;
            }
            else if (name.MatchesFrom(UTF8Constants.Duration.Span))
            {
                var parseResult = PeriodPattern.Roundtrip.Parse(value.ToString(Encoding.UTF8));
                if (parseResult.Success)
                    nextEvent.Duration = parseResult.Value.ToDuration();
            }
            else if (name.MatchesFrom(UTF8Constants.Summary.Span))
            {
                nextEvent.Summary = value.ToString(Encoding.UTF8);
            }
            else if (name.MatchesFrom(UTF8Constants.Status.Span))
            {
                nextEvent.Status = EventStatusParser.Parse(value);
            }
        }
    }
}
