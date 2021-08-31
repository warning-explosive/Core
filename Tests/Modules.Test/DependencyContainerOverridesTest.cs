namespace SpaceEngineers.Core.Modules.Test
{
    using System.Linq;
    using AutoRegistration.Api.Abstractions;
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
        internal void OverrideResolvableTest()
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

            Assert.Equal(typeof(ScopedServiceOverride), Resolve(Fixture, OverrideResolvable<ScopedServiceOverride>(Fixture, EnLifestyle.Scoped), typeProvider).GetType());
            Assert.Equal(typeof(ScopedServiceSingletonOverride), Resolve(Fixture, OverrideResolvable<ScopedServiceSingletonOverride>(Fixture, EnLifestyle.Singleton), typeProvider).GetType());
            Assert.Throws<ContainerConfigurationException>(() => Resolve(Fixture, OverrideResolvable<ScopedServiceTransientOverride>(Fixture, EnLifestyle.Transient), typeProvider).GetType());

            static DependencyContainerOptions OverrideResolvable<TOverride>(
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
        internal void OverrideCollectionResolvableTest()
        {
            var assemblies = new[]
            {
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.CompositionRoot)))
            };

            var additionalOurTypes = new[]
            {
                typeof(IScopedCollectionResolvable),
                typeof(ScopedCollectionResolvable),
                typeof(ScopedCollectionResolvableOverride),
                typeof(ScopedCollectionResolvableSingletonOverride),
                typeof(ScopedCollectionResolvableTransientOverride)
            };

            var typeProvider = TypeProvider
                .CreateBoundedAbove(assemblies)
                .ExtendTypeProvider(additionalOurTypes);

            Assert.Equal(typeof(ScopedCollectionResolvableOverride), Resolve(Fixture, OverrideCollectionResolvable<ScopedCollectionResolvableOverride>(Fixture, EnLifestyle.Scoped), typeProvider).GetType());
            Assert.Equal(typeof(ScopedCollectionResolvableSingletonOverride), Resolve(Fixture, OverrideCollectionResolvable<ScopedCollectionResolvableSingletonOverride>(Fixture, EnLifestyle.Singleton), typeProvider).GetType());
            Assert.Throws<ContainerConfigurationException>(() => Resolve(Fixture, OverrideCollectionResolvable<ScopedCollectionResolvableTransientOverride>(Fixture, EnLifestyle.Transient), typeProvider).GetType());

            static DependencyContainerOptions OverrideCollectionResolvable<TOverride>(
                ModulesTestFixture fixture,
                EnLifestyle lifestyle)
                where TOverride : IScopedCollectionResolvable
            {
                var overrides = fixture.DelegateOverride(container =>
                {
                    container.Override<IScopedCollectionResolvable, ScopedCollectionResolvable, TOverride>(lifestyle);
                });

                return new DependencyContainerOptions().WithOverrides(overrides);
            }

            static IScopedCollectionResolvable Resolve(
                ModulesTestFixture fixture,
                DependencyContainerOptions options,
                ITypeProvider typeProvider)
            {
                var dependencyContainer = fixture.CreateContainer(options, typeProvider);

                using (dependencyContainer.OpenScope())
                {
                    return dependencyContainer
                        .ResolveCollection<IScopedCollectionResolvable>()
                        .Single();
                }
            }
        }

        [Fact]
        internal void OverrideDecoratorTest()
        {
            var assemblies = new[]
            {
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.CompositionRoot)))
            };

            var additionalOurTypes = new[]
            {
                typeof(IScopedCollectionResolvable),
                typeof(ScopedCollectionResolvable),
                typeof(ScopedCollectionResolvableDecorator)
            };

            var typeProvider = TypeProvider
                .CreateBoundedAbove(assemblies)
                .ExtendTypeProvider(additionalOurTypes);

            // 1 - override decoratee
            var instance = Resolve(Fixture, OverrideDecoratee<ScopedCollectionResolvableOverride>(Fixture, EnLifestyle.Scoped), typeProvider);
            Assert.Equal(typeof(ScopedCollectionResolvableDecorator), instance.GetType());
            Assert.Equal(typeof(ScopedCollectionResolvableOverride), Decoratee(instance).GetType());

            instance = Resolve(Fixture, OverrideDecoratee<ScopedCollectionResolvableSingletonOverride>(Fixture, EnLifestyle.Singleton), typeProvider);
            Assert.Equal(typeof(ScopedCollectionResolvableDecorator), instance.GetType());
            Assert.Equal(typeof(ScopedCollectionResolvableSingletonOverride), Decoratee(instance).GetType());

            Assert.Throws<ContainerConfigurationException>(() => Resolve(Fixture, OverrideDecoratee<ScopedCollectionResolvableTransientOverride>(Fixture, EnLifestyle.Transient), typeProvider));

            // 2 - override decorator
            instance = Resolve(Fixture, OverrideDecorator<ScopedCollectionResolvableDecoratorOverride>(Fixture, EnLifestyle.Scoped), typeProvider);
            Assert.Equal(typeof(ScopedCollectionResolvableDecoratorOverride), instance.GetType());
            Assert.Equal(typeof(ScopedCollectionResolvable), Decoratee(instance).GetType());

            Assert.Throws<ContainerConfigurationException>(() => instance = Resolve(Fixture, OverrideDecorator<ScopedCollectionResolvableDecoratorSingletonOverride>(Fixture, EnLifestyle.Singleton), typeProvider));

            instance = Resolve(Fixture, OverrideDecorator<ScopedCollectionResolvableDecoratorTransientOverride>(Fixture, EnLifestyle.Transient), typeProvider);
            Assert.Equal(typeof(ScopedCollectionResolvableDecoratorTransientOverride), instance.GetType());
            Assert.Equal(typeof(ScopedCollectionResolvable), Decoratee(instance).GetType());

            static DependencyContainerOptions OverrideDecoratee<TOverride>(
                ModulesTestFixture fixture,
                EnLifestyle lifestyle)
                where TOverride : IScopedCollectionResolvable
            {
                var registrations = fixture.DelegateRegistration(container =>
                {
                    container.RegisterDecorator<IScopedCollectionResolvable, ScopedCollectionResolvableDecorator>(EnLifestyle.Scoped);
                });

                var overrides = fixture.DelegateOverride(container =>
                {
                    container.Override<IScopedCollectionResolvable, ScopedCollectionResolvable, TOverride>(lifestyle);
                });

                return new DependencyContainerOptions()
                    .WithManualRegistrations(registrations)
                    .WithOverrides(overrides);
            }

            static DependencyContainerOptions OverrideDecorator<TOverride>(
                ModulesTestFixture fixture,
                EnLifestyle lifestyle)
                where TOverride : IScopedCollectionResolvable, IDecorator<IScopedCollectionResolvable>
            {
                var registrations = fixture.DelegateRegistration(container =>
                {
                    container.RegisterDecorator<IScopedCollectionResolvable, ScopedCollectionResolvableDecorator>(EnLifestyle.Scoped);
                });

                var overrides = fixture.DelegateOverride(container =>
                {
                    container.Override<IScopedCollectionResolvable, ScopedCollectionResolvableDecorator, TOverride>(lifestyle);
                });

                return new DependencyContainerOptions()
                    .WithManualRegistrations(registrations)
                    .WithOverrides(overrides);
            }

            static IScopedCollectionResolvable Resolve(
                ModulesTestFixture fixture,
                DependencyContainerOptions options,
                ITypeProvider typeProvider)
            {
                var dependencyContainer = fixture.CreateContainer(options, typeProvider);

                using (dependencyContainer.OpenScope())
                {
                    return dependencyContainer
                        .ResolveCollection<IScopedCollectionResolvable>()
                        .Single();
                }
            }

            static T Decoratee<T>(T service)
                where T : IScopedCollectionResolvable
            {
                return service is IDecorator<T> decorator
                    ? decorator.Decoratee
                    : service;
            }
        }
    }
}