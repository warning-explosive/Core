namespace SpaceEngineers.Core.Modules.Test
{
    using System;
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
    using Registrations;
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
        internal void OverrideManuallyRegisteredComponentTest()
        {
            var assemblies = new[]
            {
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.CompositionRoot)))
            };

            var additionalOurTypes = new[]
            {
                typeof(IScopedService),
                typeof(ScopedService),
                typeof(IManuallyRegisteredService),
                typeof(ManuallyRegisteredService),
                typeof(ManuallyRegisteredServiceOverride)
            };

            var typeProvider = TypeProvider
                .CreateBoundedAbove(assemblies)
                .ExtendTypeProvider(additionalOurTypes);

            // register & override
            {
                var options = OverrideManuallyRegistered(Fixture).WithManualRegistrations(new ManuallyRegisteredServiceManualRegistration());
                var container = Fixture.CreateContainer(options, typeProvider);

                Assert.Equal(typeof(ManuallyRegisteredServiceOverride), container.Resolve<IManuallyRegisteredService>().GetType());
                Assert.Equal(typeof(ManuallyRegisteredService), container.Resolve<ManuallyRegisteredService>().GetType());
                Assert.Equal(typeof(ManuallyRegisteredServiceOverride), container.Resolve<ManuallyRegisteredServiceOverride>().GetType());
            }

            // override without original registration
            {
                var options = OverrideManuallyRegistered(Fixture);
                Assert.Throws<ContainerConfigurationException>(() => Fixture.CreateContainer(options, typeProvider));
            }

            static DependencyContainerOptions OverrideManuallyRegistered(ModulesTestFixture fixture)
            {
                var registration = fixture.DelegateRegistration(container =>
                {
                    container.Register<ManuallyRegisteredServiceOverride, ManuallyRegisteredServiceOverride>(EnLifestyle.Transient);
                });

                var overrides = fixture.DelegateOverride(container =>
                {
                    container.Override<IManuallyRegisteredService, ManuallyRegisteredService, ManuallyRegisteredServiceOverride>(EnLifestyle.Transient);
                });

                return new DependencyContainerOptions()
                    .WithManualRegistrations(registration)
                    .WithOverrides(overrides);
            }
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

        [Fact]
        internal void OverrideInstanceTest()
        {
            var assemblies = new[]
            {
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.CompositionRoot)))
            };

            var additionalOurTypes = new[]
            {
                typeof(IScopedService),
                typeof(ScopedService),
                typeof(IManuallyRegisteredService),
                typeof(ManuallyRegisteredService),
                typeof(ManuallyRegisteredServiceOverride)
            };

            var typeProvider = TypeProvider
                .CreateBoundedAbove(assemblies)
                .ExtendTypeProvider(additionalOurTypes);

            var original = new ManuallyRegisteredService();

            var registration = Fixture.DelegateRegistration(container =>
            {
                container.RegisterInstance<IManuallyRegisteredService>(original);
                container.RegisterInstance<ManuallyRegisteredService>(original);
            });

            var replacement = new ManuallyRegisteredServiceOverride();

            var overrides = Fixture.DelegateOverride(container =>
            {
                container.OverrideInstance<IManuallyRegisteredService>(replacement);
            });

            var options = new DependencyContainerOptions()
                .WithManualRegistrations(registration)
                .WithOverrides(overrides);

            var dependencyContainer = Fixture.CreateContainer(options, typeProvider);
            var resolved = dependencyContainer.Resolve<IManuallyRegisteredService>();

            Assert.NotSame(original, replacement);
            Assert.NotSame(original, resolved);
            Assert.Same(replacement, resolved);
            Assert.Same(resolved, dependencyContainer.Resolve<IManuallyRegisteredService>());
        }

        [Fact]
        internal void OverrideDelegateTest()
        {
            var assemblies = new[]
            {
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.CompositionRoot)))
            };

            var additionalOurTypes = new[]
            {
                typeof(IScopedService),
                typeof(ScopedService),
                typeof(IManuallyRegisteredService),
                typeof(ManuallyRegisteredService),
                typeof(ManuallyRegisteredServiceOverride)
            };

            var typeProvider = TypeProvider
                .CreateBoundedAbove(assemblies)
                .ExtendTypeProvider(additionalOurTypes);

            var originalFactory = new Func<ManuallyRegisteredService>(() => new ManuallyRegisteredService());

            var registration = Fixture.DelegateRegistration(container =>
            {
                container.Advanced.RegisterDelegate<IManuallyRegisteredService>(originalFactory, EnLifestyle.Singleton);
                container.Advanced.RegisterDelegate<ManuallyRegisteredService>(originalFactory, EnLifestyle.Singleton);
            });

            var replacementFactory = new Func<IManuallyRegisteredService>(() => new ManuallyRegisteredServiceOverride());

            var overrides = Fixture.DelegateOverride(container =>
            {
                container.OverrideDelegate<IManuallyRegisteredService>(replacementFactory, EnLifestyle.Singleton);
            });

            var options = new DependencyContainerOptions()
                .WithManualRegistrations(registration)
                .WithOverrides(overrides);

            var dependencyContainer = Fixture.CreateContainer(options, typeProvider);
            var resolved = dependencyContainer.Resolve<IManuallyRegisteredService>();

            Assert.Equal(typeof(ManuallyRegisteredServiceOverride), resolved.GetType());
            Assert.Same(resolved, dependencyContainer.Resolve<IManuallyRegisteredService>());
        }
    }
}