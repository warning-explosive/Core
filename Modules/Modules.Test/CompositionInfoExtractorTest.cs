namespace SpaceEngineers.Core.Modules.Test
{
    using System.Linq;
    using AutoWiringApi.Services;
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

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        internal void CompositionInfoTest(bool mode)
        {
            using (DependencyContainer.OpenScope())
            {
                var compositionInfo = DependencyContainer.Resolve<ICompositionInfoExtractor>()
                                                         .GetCompositionInfo(mode)
                                                         .ToArray();

                Output.WriteLine($"Total: {compositionInfo.Length}\n");

                Output.WriteLine(DependencyContainer.Resolve<ICompositionInfoInterpreter<string>>()
                                                    .Visualize(compositionInfo));
            }
        }
    }
}