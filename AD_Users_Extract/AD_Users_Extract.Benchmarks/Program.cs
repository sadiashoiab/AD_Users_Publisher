using System.Diagnostics.CodeAnalysis;
using BenchmarkDotNet.Running;

namespace AD_Users_Extract.Benchmarks
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
