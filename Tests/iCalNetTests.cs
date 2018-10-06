using Khronos.iCal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Tests
{
    public class iCalNetTests
    {
        [Fact]
        public void CanParseEvents()
        {
            var file = new FileInfo(Path.Combine(AppContext.BaseDirectory, "Data", "basic.ics"));

            var events = iCalNetParser.LoadAndParseFeed(file);

            Assert.Equal(1153, events.Count);
        }
    }
}
