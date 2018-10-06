using BenchmarkDotNet.Running;
using System;

namespace Benchmarks
{
    public class Program
    {
        static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<iCalParsers>();
        }
    }
}
