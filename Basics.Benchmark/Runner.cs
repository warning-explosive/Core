namespace Basics.Benchmark
{
    using Sources;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// Benchmark runner
    /// </summary>
    public class Runner
    {
        /// <summary> .cctor </summary>
        /// <param name="output">ITestOutputHelper</param>
        public Runner(ITestOutputHelper output)
        {
            Output = output;
        }

        private ITestOutputHelper Output { get; }

        [Fact]
        internal void DeepCopyBenchmark()
        {
            var summary = BenchmarkRunnerExtensions.Run<DeepCopyBenchmarkSource>(Output.WriteLine);
            var measures = summary.Measures("Median", Output.WriteLine);

            var bySerialization = measures[nameof(DeepCopyBenchmarkSource.DeepCopyBySerialization)];
            var byReflection = measures[nameof(DeepCopyBenchmarkSource.DeepCopyByReflection)];
            var multiplier = (int)(bySerialization / byReflection);

            Output.WriteLine($"{nameof(bySerialization)}: {bySerialization}");
            Output.WriteLine($"{nameof(byReflection)}: {byReflection}");
            Output.WriteLine($"{nameof(multiplier)}: {multiplier}");

            // TODO: remove magic numbers and use adaptive approach
            Assert.True(multiplier >= 5);
        }
    }
}