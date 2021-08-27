namespace SpaceEngineers.Core.Basics.Benchmark
{
    using Api;
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
            var summary = Benchmark.Run<DeepCopyBenchmarkSource>(Output.WriteLine);

            var bySerialization = summary.NanosecondMeasure(
                nameof(DeepCopyBenchmarkSource.DeepCopyBySerialization),
                Measure.Mean,
                Output.WriteLine);

            var byReflection = summary.NanosecondMeasure(
                nameof(DeepCopyBenchmarkSource.DeepCopyByReflection),
                Measure.Mean,
                Output.WriteLine);

            var multiplier = bySerialization / byReflection;

            Output.WriteLine($"{nameof(bySerialization)}: {bySerialization}");
            Output.WriteLine($"{nameof(byReflection)}: {byReflection}");
            Output.WriteLine($"{nameof(multiplier)}: {multiplier:N}");

            Assert.True(multiplier >= 3m);
        }

        [Fact]
        internal void AssembliesExtensionsBelowBenchmark()
        {
            var summary = Benchmark.Run<AssembliesExtensionsBelowBenchmarkSource>(Output.WriteLine);

            var measure = summary.MillisecondMeasure(
                nameof(AssembliesExtensionsBelowBenchmarkSource.Below),
                Measure.Mean,
                Output.WriteLine);

            Assert.True(measure <= 25m);
        }
    }
}