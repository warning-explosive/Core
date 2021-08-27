namespace SpaceEngineers.Core.Modules.Benchmark
{
    using Basics.Benchmark.Api;
    using Core.Test.Api;
    using Core.Test.Api.ClassFixtures;
    using Sources;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// Benchmarks
    /// </summary>
    // TODO: remove magic numbers and use adaptive approach -> store test artifacts in DB and search performance change points
    public class Benchmarks : TestBase
    {
        /// <summary> .cctor </summary>
        /// <param name="output">ITestOutputHelper</param>
        /// <param name="fixture">ModulesTestFixture</param>
        public Benchmarks(ITestOutputHelper output, ModulesTestFixture fixture)
            : base(output, fixture)
        {
        }

        [Fact]
        internal void CompositionRootStartupBenchmark()
        {
            var summary = Benchmark.Run<CompositionRootStartupBenchmarkSource>(Output.WriteLine);

            var measure = summary.MillisecondMeasure(
                nameof(CompositionRootStartupBenchmarkSource.CreateBoundedAbove),
                Measure.Mean,
                Output.WriteLine);

            Assert.True(measure <= 1000m);
        }
    }
}