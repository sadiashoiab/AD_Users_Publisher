using System.Diagnostics.CodeAnalysis;
using BenchmarkDotNet.Running;

namespace Azure_AD_Users_Extract.Benchmarks
{
    [ExcludeFromCodeCoverage]
    class Program
    {
        static void Main()
        {
            var _ = BenchmarkRunner.Run<UserService>();
        }
    }
}
