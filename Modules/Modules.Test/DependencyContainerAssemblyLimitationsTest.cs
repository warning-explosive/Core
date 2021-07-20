namespace SpaceEngineers.Core.Modules.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoRegistration;
    using AutoRegistration.Abstractions;
    using AutoWiring.Api;
    using AutoWiring.Api.Services;
    using Basics;
    using Core.Test.Api;
    using Core.Test.Api.ClassFixtures;
    using CrossCuttingConcerns.Api.Abstractions;
    using Mocks;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// IDependencyContainer and assembly limitations test
    /// </summary>
    public class DependencyContainerAssemblyLimitationsTest : TestBase
    {
        /// <summary> .ctor </summary>
        /// <param name="output">ITestOutputHelper</param>
        /// <param name="fixture">ModulesTestFixture</param>
        public DependencyContainerAssemblyLimitationsTest(ITestOutputHelper output, ModulesTestFixture fixture)
            : base(output, fixture)
        {
        }

        [Fact]
        internal void BoundedAboveContainerTest()
        {
            var assembly1 = AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.CrossCuttingConcerns)));
            var assembly2 = AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.PathResolver)));
            var assembly3 = AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.GenericEndpoint), nameof(Core.GenericEndpoint.Contract)));

            var below1 = AssembliesExtensions.AllAssembliesFromCurrentDomain().Below(assembly1);
            var below2 = AssembliesExtensions.AllAssembliesFromCurrentDomain().Below(assembly2);
            var below3 = AssembliesExtensions.AllAssembliesFromCurrentDomain().Below(assembly3);

            Assert.DoesNotContain(assembly1, below2);
            Assert.DoesNotContain(assembly1, below3);

            Assert.DoesNotContain(assembly2, below1);
            Assert.DoesNotContain(assembly2, below3);

            Assert.DoesNotContain(assembly3, below1);
            Assert.DoesNotContain(assembly3, below2);

            var allowedAssemblies = new[]
            {
                assembly1,
                assembly2,
                assembly3,

                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.Basics))),
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.AutoWiring), nameof(Core.AutoWiring.Api))),
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.AutoRegistration))),
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.CrossCuttingConcerns), nameof(Core.CrossCuttingConcerns.Api)))
            };

            var aboveAssemblies = new[]
            {
                assembly1,
                assembly2,
                assembly3
            };

            var ourTypes = Fixture
                .BoundedAboveContainer(new DependencyContainerOptions(), aboveAssemblies)
                .Resolve<ITypeProvider>()
                .OurTypes;

            Assert.True(ourTypes.All(Satisfies));

            bool Satisfies(Type type)
            {
                var satisfies = allowedAssemblies.Contains(type.Assembly);

                if (!satisfies)
                {
                    Output.WriteLine(type.FullName);
                }

                return satisfies;
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        internal void ExactlyBoundedContainerTest(bool mode)
        {
            var assemblies = new[]
            {
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.AutoWiring), nameof(Core.AutoWiring.Api))),
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.CrossCuttingConcerns), nameof(Core.CrossCuttingConcerns.Api))),
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.CrossCuttingConcerns)))
            };

            var boundedContainer = Fixture.ExactlyBoundedContainer(new DependencyContainerOptions(), assemblies);

            if (mode)
            {
                Assert.Throws<InvalidOperationException>(() => GetCompositionInfo(boundedContainer, mode));
            }
            else
            {
                _ = GetCompositionInfo(boundedContainer, mode);
            }

            var additionalTypes = new[]
            {
                typeof(TestJsonSettings),
                typeof(TestYamlSettings),
                typeof(ExtendedTypeProviderDecorator)
            };

            var options = ExtendedTypeProviderDecorator.ExtendTypeProvider(new DependencyContainerOptions(), additionalTypes);

            var extendedBoundedContainer = Fixture.ExactlyBoundedContainer(options, assemblies);

            var compositionInfo = GetCompositionInfo(extendedBoundedContainer, mode);

            Output.WriteLine($"Total: {compositionInfo.Count}\n");
            Output.WriteLine(boundedContainer.Resolve<ICompositionInfoInterpreter<string>>().Visualize(compositionInfo));

            var allowedAssemblies = new[]
            {
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SimpleInjector))),
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(Newtonsoft), nameof(Newtonsoft.Json))),
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(YamlDotNet))),
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.Basics))),
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.AutoWiring), nameof(Core.AutoWiring.Api))),
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.AutoRegistration))),
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.CrossCuttingConcerns), nameof(Core.CrossCuttingConcerns.Api))),
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.CrossCuttingConcerns))),
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
                                || type == typeof(ExtendedTypeProviderDecorator)
                                || type == typeof(ExtendedTypeProviderDecorator.TypeProviderExtension);

                if (!satisfies)
                {
                    Output.WriteLine(type.FullName);
                }

                return satisfies;
            }

            static IReadOnlyCollection<IDependencyInfo> GetCompositionInfo(IDependencyContainer container, bool mode)
            {
                return container.Resolve<ICompositionInfoExtractor>().GetCompositionInfo(mode);
            }
        }

        private class TestYamlSettings : IYamlSettings
        {
        }

        private class TestJsonSettings : IJsonSettings
        {
        }
    }
}