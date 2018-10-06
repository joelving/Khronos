using BenchmarkDotNet.Attributes;
using Khronos.iCal;
using Khronos.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Benchmarks
{
    [MemoryDiagnoser]
    public class iCalParsers
    {
        private FileInfo file;

        [GlobalSetup]
        public void Setup()
        {
            file = new FileInfo(Path.Combine(AppContext.BaseDirectory, "Data", "basic.ics"));
        }

        [Benchmark(Baseline = true)]
        public Task<List<Event>> ParseEventWithPipes()
        {
            return UTF8Parser.LoadAndParseFeed(file);
        }

        [Benchmark]
        public List<Event> ParseEventsICalNet()
        {
            return iCalNetParser.LoadAndParseFeed(file);
        }
    }
}
