namespace SpaceEngineers.Core.GenericHost.Benchmark
{
    using System.Threading.Tasks;
    using Core.Benchmark.Api;
    using Sources;
    using Test.Api;
    using Test.Api.ClassFixtures;
    using Xunit;
    using Xunit.Abstractions;
    using Benchmark = SpaceEngineers.Core.Benchmark.Api.Benchmark;

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
            StaticOutput = output;
        }

        /// <summary>
        /// StaticOutput
        /// </summary>
        public static ITestOutputHelper? StaticOutput { get; private set; }

        [Fact(Timeout = 300_000)]
        internal void MessageHandlerMiddlewareBenchmark()
        {
            var summary = Benchmark.Run<MessageHandlerMiddlewareBenchmarkSource>(Output.WriteLine);

            var compositeMiddleware = summary.MillisecondMeasure(
                nameof(MessageHandlerMiddlewareBenchmarkSource.RunCompositeMiddleware),
                Measure.Mean,
                Output.WriteLine);

            var errorHandlingMiddleware = summary.MillisecondMeasure(
                nameof(MessageHandlerMiddlewareBenchmarkSource.RunErrorHandlingMiddleware),
                Measure.Mean,
                Output.WriteLine);

            var authorizationMiddleware = summary.MillisecondMeasure(
                nameof(MessageHandlerMiddlewareBenchmarkSource.RunAuthorizationMiddleware),
                Measure.Mean,
                Output.WriteLine);

            var unitOfWorkMiddleware = summary.MillisecondMeasure(
                nameof(MessageHandlerMiddlewareBenchmarkSource.RunUnitOfWorkMiddleware),
                Measure.Mean,
                Output.WriteLine);

            var handledByEndpointMiddleware = summary.MillisecondMeasure(
                nameof(MessageHandlerMiddlewareBenchmarkSource.RunHandledByEndpointMiddleware),
                Measure.Mean,
                Output.WriteLine);

            var requestReplyMiddleware = summary.MillisecondMeasure(
                nameof(MessageHandlerMiddlewareBenchmarkSource.RunRequestReplyMiddleware),
                Measure.Mean,
                Output.WriteLine);

            Assert.True(compositeMiddleware < 10m);
            Assert.True(errorHandlingMiddleware < 1m);
            Assert.True(authorizationMiddleware < 1m);
            Assert.True(unitOfWorkMiddleware < 1m);
            Assert.True(handledByEndpointMiddleware < 1m);
            Assert.True(requestReplyMiddleware < 1m);
        }

        [Fact(Timeout = 300_000)]
        internal async Task MessageHandlerMiddlewareBenchmarkTest()
        {
            var source = new MessageHandlerMiddlewareBenchmarkSource();

            try
            {
                source.GlobalSetup();

                for (var i = 0; i < 1000; i++)
                {
                    await source.RunErrorHandlingMiddleware().ConfigureAwait(false);
                    await source.RunAuthorizationMiddleware().ConfigureAwait(false);
                    await source.RunUnitOfWorkMiddleware().ConfigureAwait(false);
                    await source.RunHandledByEndpointMiddleware().ConfigureAwait(false);
                    await source.RunRequestReplyMiddleware().ConfigureAwait(false);

                    source.IterationCleanup();
                }
            }
            finally
            {
                source.GlobalCleanup();
            }
        }
    }
}