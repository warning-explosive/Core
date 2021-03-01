namespace Basics.Benchmark
{
    using Sources;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// Benchmarks
    /// </summary>
    // TODO: remove magic numbers and use adaptive approach -> store test artifacts in DB and search performance change points
    public class Benchmarks
    {
        /// <summary> .cctor </summary>
        /// <param name="output">ITestOutputHelper</param>
        public Benchmarks(ITestOutputHelper output)
        {
            Output = output;
        }

        private ITestOutputHelper Output { get; }

        [Fact]
        internal void DeepCopyBenchmark()
        {
            var summary = BenchmarkRunnerExtensions.Run<DeepCopyBenchmarkSource>(Output.WriteLine);
            var measures = summary.NanosecondMeasures("Mean", Output.WriteLine);

            var bySerialization = measures[nameof(DeepCopyBenchmarkSource.DeepCopyBySerialization)];
            var byReflection = measures[nameof(DeepCopyBenchmarkSource.DeepCopyByReflection)];
            var multiplier = bySerialization / byReflection;

            Output.WriteLine($"{nameof(bySerialization)}: {bySerialization}");
            Output.WriteLine($"{nameof(byReflection)}: {byReflection}");
            Output.WriteLine($"{nameof(multiplier)}: {multiplier:N}");

            Assert.True(multiplier >= 5m);
        }

        [Fact]
        internal void AssembliesExtensionsBelowBenchmark()
        {
            var summary = BenchmarkRunnerExtensions.Run<AssembliesExtensionsBelowSource>(Output.WriteLine);

            var measures = summary.MillisecondMeasures("Mean", Output.WriteLine);
            var measure = measures[nameof(AssembliesExtensionsBelowSource.Below)];

            Assert.True(measure <= 25m);
        }
    }
}