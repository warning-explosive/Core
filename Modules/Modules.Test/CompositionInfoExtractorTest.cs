namespace SpaceEngineers.Core.Modules.Test
{
    using System.Linq;
    using CompositionInfoExtractor;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// CompositionInfoExtractor class tests
    /// </summary>
    public class CompositionInfoExtractorTest : ModulesTestBase
    {
        /// <summary> .ctor </summary>
        /// <param name="output">ITestOutputHelper</param>
        public CompositionInfoExtractorTest(ITestOutputHelper output)
            : base(output) { }

        [Fact]
        internal void SimpleTest()
        {
            var compositionInfo = DependencyContainer.Resolve<ICompositionInfoExtractor>()
                                                     .GetCompositionInfo()
                                                     .ToArray();

            Output.WriteLine($"Total: {compositionInfo.Length}\n");

            Output.WriteLine(DependencyContainer.Resolve<ICompositionInfoInterpreter<string>>()
                                                .Visualize(compositionInfo));
        }
    }
}