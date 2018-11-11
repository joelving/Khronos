using Khronos.iCal;
using NodaTime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Tests
{
    public class UTF8ParserTests
    {
        [Fact]
        public async Task CanParseEvents()
        {
            var file = new FileInfo(Path.Combine(AppContext.BaseDirectory, "Data", "basic.ics"));

            var events = await UTF8Parser.LoadAndParseFeed(file);

            Assert.Equal(1159, events.Count);
        }

        [Fact]
        public async Task CanParseTextProperties()
        {
            var file = new FileInfo(Path.Combine(AppContext.BaseDirectory, "Data", "basic.ics"));

            var events = await UTF8Parser.LoadAndParseFeed(file);

            Assert.NotEmpty(events.Where(ev =>
                !string.IsNullOrWhiteSpace(ev.UId)
                ||
                !string.IsNullOrWhiteSpace(ev.Summary)
            ));
        }

        [Fact]
        public async Task CanParseInstants()
        {
            var file = new FileInfo(Path.Combine(AppContext.BaseDirectory, "Data", "basic.ics"));

            var events = await UTF8Parser.LoadAndParseFeed(file);

            Assert.NotEmpty(events.Where(ev =>
                ev.Start != default
                ||
                ev.End != default
            ));
        }
    }
}
