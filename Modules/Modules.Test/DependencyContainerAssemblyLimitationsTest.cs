namespace SpaceEngineers.Core.Modules.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoRegistration;
    using AutoRegistration.Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Contexts;
    using AutoWiring.Api.Services;
    using Basics;
    using Core.SettingsManager.Abstractions;
    using Core.Test.Api;
    using Core.Test.Api.ClassFixtures;
    using GenericEndpoint.Contract.Abstractions;
    using Json.Abstractions;
    using Mocks;
    using PathResolver;
    using Xunit;
    using Xunit.Abstractions;
    using Container = SimpleInjector.Container;
    using TypeExtensions = System.Reflection.TypeExtensions;

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
            var assembly1 = typeof(IJsonSerializer).Assembly; // Json
            var assembly2 = typeof(IPathResolver<,>).Assembly; // PathResolver
            var assembly3 = typeof(IIntegrationCommand).Assembly; // GenericEndpoint.Contract

            var below1 = AssembliesExtensions.AllFromCurrentDomain().Below(assembly1);
            var below2 = AssembliesExtensions.AllFromCurrentDomain().Below(assembly2);
            var below3 = AssembliesExtensions.AllFromCurrentDomain().Below(assembly3);

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

                typeof(AssembliesExtensions).Assembly, // Basics
                typeof(ComponentAttribute).Assembly, // AutoWiring.Api
                typeof(DependencyContainer).Assembly, // AutoRegistration
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
                typeof(ISettingsManager<>).Assembly,
                typeof(ICompositionInfoExtractor).Assembly
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
                typeof(TypeProviderMock)
            };

            var typeProvider = new TypeProviderMock(boundedContainer.Resolve<ITypeProvider>(), additionalTypes);
            var overrides = Fixture.DelegateRegistration(container =>
            {
                container
                    .RegisterInstance(typeof(ITypeProvider), typeProvider)
                    .RegisterInstance(typeProvider.GetType(), typeProvider);
            });

            var options = new DependencyContainerOptions().WithOverride(overrides);

            var extendedBoundedContainer = Fixture.ExactlyBoundedContainer(options, assemblies);

            var compositionInfo = GetCompositionInfo(extendedBoundedContainer, mode);

            Output.WriteLine($"Total: {compositionInfo.Count}\n");
            Output.WriteLine(boundedContainer.Resolve<ICompositionInfoInterpreter<string>>().Visualize(compositionInfo));

            var allowedAssemblies = new[]
            {
                typeof(Container).Assembly, // SimpleInjector assembly,
                typeof(TypeExtensions).Assembly, // Basics assembly
                typeof(ComponentAttribute).Assembly, // AutoWiring.Api assembly
                typeof(IDependencyContainer).Assembly, // AutoRegistration assembly
                typeof(ISettingsManager<>).Assembly, // SettingsManager assembly
                typeof(ICompositionInfoExtractor).Assembly // CompositionInfoExtractor assembly
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
                                || type == typeof(TypeProviderMock);

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