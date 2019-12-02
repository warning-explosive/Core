namespace SpaceEngineers.Core.Utilities.Test
{
    using System.Linq;
    using CompositionInfoExtractor;
    using CompositionRoot.Test;
    using Xunit;
    using Xunit.Abstractions;

    public class CompositionInfoExtractorTest : CompositionRootTestBase
    {
        public CompositionInfoExtractorTest(ITestOutputHelper output) : base(output) { }

        [Fact]
        public void SimpleTest()
        {
            var compositionInfo = DependencyContainer.Resolve<ICompositionInfoExtractor>()
                                                     .GetCompositionInfo()
                                                     .ToArray();
            
            Output.WriteLine($"Total: {compositionInfo.Length}\n");

            Output.WriteLine(DependencyContainer.Resolve<ICompositionInfoVisualizer>()
                                                .Visualize(compositionInfo));
        }
    }
}