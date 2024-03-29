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

        [Fact(Timeout = 300_000)]
        internal void DeepCopyBenchmark()
        {
            var summary = Benchmark.Run<DeepCopyBenchmarkSource>(Output.WriteLine);

            var byReflection = summary.MillisecondMeasure(
                nameof(DeepCopyBenchmarkSource.DeepCopyByReflection),
                Measure.Mean,
                Output.WriteLine);

            Assert.True(byReflection <= 50m);
        }

        [Fact(Timeout = 300_000)]
        internal void AssembliesExtensionsBelowBenchmark()
        {
            var summary = Benchmark.Run<AssembliesExtensionsBelowBenchmarkSource>(Output.WriteLine);

            var measure = summary.MillisecondMeasure(
                nameof(AssembliesExtensionsBelowBenchmarkSource.Below),
                Measure.Mean,
                Output.WriteLine);

            Assert.True(measure <= 25m);
        }

        [Fact(Timeout = 300_000)]
        internal void CompositionRootStartupBenchmark()
        {
            var summary = Benchmark.Run<CompositionRootStartupBenchmarkSource>(Output.WriteLine);

            var create = summary.MillisecondMeasure(
                nameof(CompositionRootStartupBenchmarkSource.Create),
                Measure.Mean,
                Output.WriteLine);

            Assert.True(create <= 1000m);
        }

        [Fact(Timeout = 300_000)]
        internal void StreamCopyVersusStreamReadBenchmark()
        {
            var summary = Benchmark.Run<StreamCopyVersusStreamReadBenchmarkSource>(Output.WriteLine);

            var copyTo = summary.MillisecondMeasure(
                nameof(StreamCopyVersusStreamReadBenchmarkSource.CopyTo),
                Measure.Mean,
                Output.WriteLine);

            Assert.True(copyTo <= 10m);

            var read = summary.MillisecondMeasure(
                nameof(StreamCopyVersusStreamReadBenchmarkSource.Read),
                Measure.Mean,
                Output.WriteLine);

            Assert.True(read <= 10m);
        }
    }
}