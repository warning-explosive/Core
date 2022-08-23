namespace SpaceEngineers.Core.Modules.Benchmark
{
    using Core.Benchmark.Api;
    using Sources;
    using Test.Api;
    using Test.Api.ClassFixtures;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// Benchmarks
    /// </summary>
    // TODO: #136 - remove magic numbers and use adaptive approach -> store test artifacts in DB and search performance change points
    public class Benchmarks : TestBase
    {
        /// <summary> .cctor </summary>
        /// <param name="output">ITestOutputHelper</param>
        /// <param name="fixture">TestFixture</param>
        public Benchmarks(ITestOutputHelper output, TestFixture fixture)
            : base(output, fixture)
        {
        }

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

        [Fact]
        internal void CompositionRootStartupBenchmark()
        {
            var summary = Benchmark.Run<CompositionRootStartupBenchmarkSource>(Output.WriteLine);

            var createExactlyBounded = summary.MillisecondMeasure(
                nameof(CompositionRootStartupBenchmarkSource.CreateExactlyBounded),
                Measure.Mean,
                Output.WriteLine);

            Assert.True(createExactlyBounded <= 1000m);

            var createBoundedAbove = summary.MillisecondMeasure(
                nameof(CompositionRootStartupBenchmarkSource.CreateBoundedAbove),
                Measure.Mean,
                Output.WriteLine);

            Assert.True(createBoundedAbove <= 1000m);
        }
    }
}