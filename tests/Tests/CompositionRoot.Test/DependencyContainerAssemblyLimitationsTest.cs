namespace SpaceEngineers.Core.CompositionRoot.Test
{
    using System;
    using System.Linq;
    using Basics;
    using CompositionInfo;
    using CompositionRoot;
    using Microsoft.Extensions.Configuration;
    using SpaceEngineers.Core.Test.Api;
    using SpaceEngineers.Core.Test.Api.ClassFixtures;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// DependencyContainerAssemblyLimitationsTest
    /// </summary>
    public class DependencyContainerAssemblyLimitationsTest : TestBase
    {
        /// <summary> .ctor </summary>
        /// <param name="output">ITestOutputHelper</param>
        /// <param name="fixture">TestFixture</param>
        public DependencyContainerAssemblyLimitationsTest(ITestOutputHelper output, TestFixture fixture)
            : base(output, fixture)
        {
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        internal void ExactlyBoundedContainerTest(bool mode)
        {
            var assemblies = new[]
            {
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(CliArgumentsParser)))
            };

            var options = new DependencyContainerOptions()
                .WithPluginAssemblies(assemblies);

            var boundedContainer = Fixture.DependencyContainer(options);

            _ = boundedContainer
               .Resolve<ICompositionInfoExtractor>()
               .GetCompositionInfo(mode);

            var additionalTypes = new[]
            {
                typeof(TestAdditionalType),
                typeof(IConfigurationProvider),
                typeof(ConfigurationProvider)
            };

            options = new DependencyContainerOptions()
                .WithPluginAssemblies(assemblies)
                .WithAdditionalOurTypes(additionalTypes);

            var compositionInfo = Fixture
               .DependencyContainer(options)
               .Resolve<ICompositionInfoExtractor>()
               .GetCompositionInfo(mode);

            Output.WriteLine($"Total: {compositionInfo.Count}{Environment.NewLine}");
            Output.WriteLine(boundedContainer.Resolve<ICompositionInfoInterpreter<string>>().Visualize(compositionInfo));

            var allowedAssemblies = new[]
            {
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SimpleInjector))),
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(Newtonsoft), nameof(Newtonsoft.Json))),
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Basics))),
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(AutoRegistration), nameof(AutoRegistration.Api))),
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(CompositionRoot))),
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(CliArgumentsParser))),
            };

            Assert.True(compositionInfo.All(Satisfies));

            bool Satisfies(IDependencyInfo info)
            {
                return TypeSatisfies(info.ServiceType)
                    && TypeSatisfies(info.ImplementationType)
                    && info.Dependencies.All(Satisfies);
            }

            bool TypeSatisfies(Type type)
            {
                var satisfies = allowedAssemblies.Contains(type.Assembly)
                             || additionalTypes.Contains(type);

                if (!satisfies)
                {
                    Output.WriteLine(type.FullName);
                }

                return satisfies;
            }
        }

        private class TestAdditionalType
        {
        }
    }
}