using System.Diagnostics.CodeAnalysis;
using BenchmarkDotNet.Running;

namespace TestBed.Benchmarks
{
    [ExcludeFromCodeCoverage]
    class Program
    {
        static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<UserService>();
        }
    }
}
