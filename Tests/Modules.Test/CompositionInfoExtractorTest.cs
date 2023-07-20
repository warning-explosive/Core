namespace SpaceEngineers.Core.Modules.Test
{
    using System;
    using System.Linq;
    using Basics;
    using CompositionRoot;
    using CompositionRoot.CompositionInfo;
    using Core.Test.Api;
    using Core.Test.Api.ClassFixtures;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// CompositionInfoExtractor class tests
    /// </summary>
    public class CompositionInfoExtractorTest : TestBase
    {
        /// <summary> .ctor </summary>
        /// <param name="output">ITestOutputHelper</param>
        /// <param name="fixture">TestFixture</param>
        public CompositionInfoExtractorTest(ITestOutputHelper output, TestFixture fixture)
            : base(output, fixture)
        {
            var options = new DependencyContainerOptions();

            DependencyContainer = fixture.DependencyContainer(options);
        }

        private IDependencyContainer DependencyContainer { get; }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        internal void CompositionInfoTest(bool mode)
        {
            using (DependencyContainer.OpenScope())
            {
                var compositionInfo = DependencyContainer
                   .Resolve<ICompositionInfoExtractor>()
                   .GetCompositionInfo(mode)
                   .ToArray();

                Output.WriteLine($"Total: {compositionInfo.Length}{Environment.NewLine}");

                Output.WriteLine(DependencyContainer
                   .Resolve<ICompositionInfoInterpreter<string>>()
                   .Visualize(compositionInfo));
            }
        }
    }
}