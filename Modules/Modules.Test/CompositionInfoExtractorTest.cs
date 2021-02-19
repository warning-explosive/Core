namespace SpaceEngineers.Core.Modules.Test
{
    using System;
    using System.Linq;
    using System.Reflection;
    using AutoRegistration.Abstractions;
    using AutoWiringApi.Services;
    using Basics.Test;
    using ClassFixtures;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// CompositionInfoExtractor class tests
    /// </summary>
    public class CompositionInfoExtractorTest : BasicsTestBase, IClassFixture<ModulesTestFixture>
    {
        /// <summary> .ctor </summary>
        /// <param name="output">ITestOutputHelper</param>
        /// <param name="fixture">ModulesTestFixture</param>
        public CompositionInfoExtractorTest(ITestOutputHelper output, ModulesTestFixture fixture)
            : base(output)
        {
            DependencyContainer = fixture.GetDependencyContainer(typeof(IDependencyContainer).Assembly, Array.Empty<Assembly>());
        }

        /// <summary>
        /// DependencyContainer
        /// </summary>
        protected IDependencyContainer DependencyContainer { get; }

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