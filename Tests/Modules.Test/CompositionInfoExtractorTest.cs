namespace SpaceEngineers.Core.Modules.Test
{
    using System;
    using System.Linq;
    using Basics;
    using CompositionRoot;
    using CompositionRoot.Api.Abstractions;
    using CompositionRoot.Api.Abstractions.CompositionInfo;
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
            var assembly = AssembliesExtensions.FindRequiredAssembly(
                AssembliesExtensions.BuildName(
                    nameof(SpaceEngineers),
                    nameof(Core),
                    nameof(Core.CompositionRoot)));

            var options = new DependencyContainerOptions();

            DependencyContainer = fixture.BoundedAboveContainer(output, options, assembly);
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