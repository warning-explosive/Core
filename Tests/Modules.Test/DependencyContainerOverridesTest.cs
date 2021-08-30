namespace SpaceEngineers.Core.Modules.Test
{
    using System;
    using AutoRegistration.Api.Enumerations;
    using AutoRegistrationTest;
    using Basics;
    using CompositionRoot;
    using CompositionRoot.Api.Abstractions;
    using CompositionRoot.Api.Exceptions;
    using CompositionRoot.Implementations;
    using Core.Test.Api;
    using Core.Test.Api.ClassFixtures;
    using Mocks;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// DependencyContainerOverridesTest
    /// </summary>
    public class DependencyContainerOverridesTest : TestBase
    {
        /// <summary> .ctor </summary>
        /// <param name="output">ITestOutputHelper</param>
        /// <param name="fixture">ModulesTestFixture</param>
        public DependencyContainerOverridesTest(ITestOutputHelper output, ModulesTestFixture fixture)
            : base(output, fixture)
        {
        }

        [Fact]
        internal void OverrideResolvable()
        {
            var assemblies = new[]
            {
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.CompositionRoot)))
            };

            var additionalOurTypes = new[]
            {
                typeof(IScopedService),
                typeof(ScopedService),
                typeof(ScopedServiceOverride),
                typeof(ScopedServiceSingletonOverride),
                typeof(ScopedServiceTransientOverride)
            };

            var typeProvider = TypeProvider
                .CreateBoundedAbove(assemblies)
                .ExtendTypeProvider(additionalOurTypes);

            Assert.Equal(typeof(ScopedServiceOverride), Resolve(Fixture, CreateOptions<ScopedServiceOverride>(Fixture, EnLifestyle.Scoped), typeProvider).GetType());
            Assert.Equal(typeof(ScopedServiceSingletonOverride), Resolve(Fixture, CreateOptions<ScopedServiceSingletonOverride>(Fixture, EnLifestyle.Singleton), typeProvider).GetType());
            Assert.Throws<ContainerConfigurationException>(() => Resolve(Fixture, CreateOptions<ScopedServiceTransientOverride>(Fixture, EnLifestyle.Transient), typeProvider).GetType());

            static DependencyContainerOptions CreateOptions<TOverride>(
                ModulesTestFixture fixture,
                EnLifestyle lifestyle)
                where TOverride : IScopedService
            {
                var overrides = fixture.DelegateOverride(container =>
                {
                    container.Override<IScopedService, ScopedService, TOverride>(lifestyle);
                });

                return new DependencyContainerOptions().WithOverrides(overrides);
            }

            static IScopedService Resolve(
                ModulesTestFixture fixture,
                DependencyContainerOptions options,
                ITypeProvider typeProvider)
            {
                var dependencyContainer = fixture.CreateContainer(options, typeProvider);

                using (dependencyContainer.OpenScope())
                {
                    return dependencyContainer.Resolve<IScopedService>();
                }
            }
        }

        [Fact]
        internal void OverrideCollectionResolvable()
        {
            throw new NotImplementedException();
        }

        [Fact]
        internal void OverrideDecorator()
        {
            throw new NotImplementedException();
        }
    }
}