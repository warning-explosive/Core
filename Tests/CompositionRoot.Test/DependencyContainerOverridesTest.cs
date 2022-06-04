namespace SpaceEngineers.Core.CompositionRoot.Test
{
    using System;
    using System.Linq;
    using System.Reflection;
    using Api.Exceptions;
    using AutoRegistrationTest;
    using Basics;
    using CompositionRoot;
    using Registrations;
    using SpaceEngineers.Core.AutoRegistration.Api.Abstractions;
    using SpaceEngineers.Core.AutoRegistration.Api.Enumerations;
    using SpaceEngineers.Core.Test.Api;
    using SpaceEngineers.Core.Test.Api.ClassFixtures;
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
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(CompositionRoot)))
            };

            var additionalOurTypes = new[]
            {
                typeof(IScopedService),
                typeof(ScopedService),
                typeof(IManuallyRegisteredService),
                typeof(ManuallyRegisteredService),
                typeof(ManuallyRegisteredServiceOverride)
            };

            // register & override
            {
                var options = OverrideManuallyRegistered(Fixture)
                    .WithManualRegistrations(new ManuallyRegisteredServiceManualRegistration())
                    .WithAdditionalOurTypes(additionalOurTypes);

                var container = Fixture.BoundedAboveContainer(Output, options, assemblies);

                Assert.Equal(typeof(ManuallyRegisteredServiceOverride), container.Resolve<IManuallyRegisteredService>().GetType());
                Assert.Equal(typeof(ManuallyRegisteredService), container.Resolve<ManuallyRegisteredService>().GetType());
                Assert.Equal(typeof(ManuallyRegisteredServiceOverride), container.Resolve<ManuallyRegisteredServiceOverride>().GetType());
            }

            // override without original registration
            {
                var options = OverrideManuallyRegistered(Fixture);
                Assert.Throws<ContainerConfigurationException>(() => Fixture.BoundedAboveContainer(Output, options, assemblies));
            }

            static DependencyContainerOptions OverrideManuallyRegistered(ModulesTestFixture fixture)
            {
                var registration = fixture.DelegateRegistration(container =>
                {
                    container.Register<ManuallyRegisteredServiceOverride, ManuallyRegisteredServiceOverride>(EnLifestyle.Transient);
                });

                var overrides = fixture.DelegateOverride(container =>
                {
                    container.Override<IManuallyRegisteredService, ManuallyRegisteredServiceOverride>(EnLifestyle.Transient);
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
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(CompositionRoot)))
            };

            var additionalOurTypes = new[]
            {
                typeof(IScopedService),
                typeof(ScopedService),
                typeof(ScopedServiceOverride),
                typeof(ScopedServiceSingletonOverride),
                typeof(ScopedServiceTransientOverride)
            };

            var resolve = Resolve(Output, Fixture, assemblies);

            Assert.Equal(typeof(ScopedServiceOverride), resolve(OverrideResolvable<ScopedServiceOverride>(Fixture, additionalOurTypes, EnLifestyle.Scoped)).GetType());
            Assert.Equal(typeof(ScopedServiceSingletonOverride), resolve(OverrideResolvable<ScopedServiceSingletonOverride>(Fixture, additionalOurTypes, EnLifestyle.Singleton)).GetType());
            Assert.Throws<ContainerConfigurationException>(() => resolve(OverrideResolvable<ScopedServiceTransientOverride>(Fixture, additionalOurTypes, EnLifestyle.Transient)).GetType());

            static DependencyContainerOptions OverrideResolvable<TOverride>(
                ModulesTestFixture fixture,
                Type[] additionalOurTypes,
                EnLifestyle lifestyle)
                where TOverride : IScopedService
            {
                var overrides = fixture.DelegateOverride(container =>
                {
                    container.Override<IScopedService, TOverride>(lifestyle);
                });

                return new DependencyContainerOptions()
                    .WithOverrides(overrides)
                    .WithAdditionalOurTypes(additionalOurTypes);
            }

            static Func<DependencyContainerOptions, IScopedService> Resolve(
                ITestOutputHelper output,
                ModulesTestFixture fixture,
                Assembly[] assemblies)
            {
                return options =>
                {
                    var dependencyContainer = fixture.BoundedAboveContainer(output, options, assemblies);

                    using (dependencyContainer.OpenScope())
                    {
                        return dependencyContainer.Resolve<IScopedService>();
                    }
                };
            }
        }

        [Fact]
        internal void OverrideCollectionResolvableTest()
        {
            var assemblies = new[]
            {
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(CompositionRoot)))
            };

            var additionalOurTypes = new[]
            {
                typeof(IScopedCollectionResolvable),
                typeof(ScopedCollectionResolvable),
                typeof(ScopedCollectionResolvableOverride),
                typeof(ScopedCollectionResolvableSingletonOverride),
                typeof(ScopedCollectionResolvableTransientOverride)
            };

            var resolve = Resolve(Output, Fixture, assemblies);

            Assert.Equal(typeof(ScopedCollectionResolvableOverride), resolve(OverrideCollectionResolvable<ScopedCollectionResolvableOverride>(Fixture, additionalOurTypes, EnLifestyle.Scoped)).GetType());
            Assert.Equal(typeof(ScopedCollectionResolvableSingletonOverride), resolve(OverrideCollectionResolvable<ScopedCollectionResolvableSingletonOverride>(Fixture, additionalOurTypes, EnLifestyle.Singleton)).GetType());
            Assert.Equal(typeof(ScopedCollectionResolvableTransientOverride), resolve(OverrideCollectionResolvable<ScopedCollectionResolvableTransientOverride>(Fixture, additionalOurTypes, EnLifestyle.Transient)).GetType());

            static DependencyContainerOptions OverrideCollectionResolvable<TOverride>(
                ModulesTestFixture fixture,
                Type[] additionalOurTypes,
                EnLifestyle lifestyle)
                where TOverride : IScopedCollectionResolvable
            {
                var overrides = fixture.DelegateOverride(container =>
                {
                    container.OverrideCollection(
                        Enumerable.Empty<object>(),
                        new[] { (typeof(TOverride), lifestyle) },
                        Enumerable.Empty<(Func<IScopedCollectionResolvable>, EnLifestyle)>());
                });

                return new DependencyContainerOptions()
                    .WithOverrides(overrides)
                    .WithAdditionalOurTypes(additionalOurTypes);
            }

            static Func<DependencyContainerOptions, IScopedCollectionResolvable> Resolve(
                ITestOutputHelper output,
                ModulesTestFixture fixture,
                Assembly[] assemblies)
            {
                return options =>
                {
                    var dependencyContainer = fixture.BoundedAboveContainer(output, options, assemblies);

                    using (dependencyContainer.OpenScope())
                    {
                        return dependencyContainer
                            .ResolveCollection<IScopedCollectionResolvable>()
                            .Single();
                    }
                };
            }
        }

        [Fact]
        internal void OverrideDecoratorTest()
        {
            var assemblies = new[]
            {
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(CompositionRoot)))
            };

            var additionalOurTypes = new[]
            {
                typeof(IScopedCollectionResolvable),
                typeof(ScopedCollectionResolvable),
                typeof(ScopedCollectionResolvableDecorator)
            };

            var resolve = Resolve(Output, Fixture, assemblies);

            // 1 - override decoratee
            var instance = resolve(OverrideDecoratee<ScopedCollectionResolvableOverride>(Fixture, additionalOurTypes, EnLifestyle.Scoped));
            Assert.Equal(typeof(ScopedCollectionResolvableDecorator), instance.GetType());
            Assert.Equal(typeof(ScopedCollectionResolvableOverride), Decoratee(instance).GetType());

            instance = resolve(OverrideDecoratee<ScopedCollectionResolvableSingletonOverride>(Fixture, additionalOurTypes, EnLifestyle.Singleton));
            Assert.Equal(typeof(ScopedCollectionResolvableDecorator), instance.GetType());
            Assert.Equal(typeof(ScopedCollectionResolvableSingletonOverride), Decoratee(instance).GetType());

            Assert.Throws<ContainerConfigurationException>(() => resolve(OverrideDecoratee<ScopedCollectionResolvableTransientOverride>(Fixture, additionalOurTypes, EnLifestyle.Transient)));

            // 2 - override decorator
            instance = resolve(OverrideDecorator<ScopedCollectionResolvableDecoratorOverride>(Fixture, additionalOurTypes, EnLifestyle.Scoped));
            Assert.Equal(typeof(ScopedCollectionResolvableDecoratorOverride), instance.GetType());
            Assert.Equal(typeof(ScopedCollectionResolvable), Decoratee(instance).GetType());

            Assert.Throws<ContainerConfigurationException>(() => instance = resolve(OverrideDecorator<ScopedCollectionResolvableDecoratorSingletonOverride>(Fixture, additionalOurTypes, EnLifestyle.Singleton)));

            instance = resolve(OverrideDecorator<ScopedCollectionResolvableDecoratorTransientOverride>(Fixture, additionalOurTypes, EnLifestyle.Transient));
            Assert.Equal(typeof(ScopedCollectionResolvableDecoratorTransientOverride), instance.GetType());
            Assert.Equal(typeof(ScopedCollectionResolvable), Decoratee(instance).GetType());

            static DependencyContainerOptions OverrideDecoratee<TOverride>(
                ModulesTestFixture fixture,
                Type[] additionalOurTypes,
                EnLifestyle lifestyle)
                where TOverride : IScopedCollectionResolvable
            {
                var registrations = fixture.DelegateRegistration(container =>
                {
                    container.RegisterDecorator<IScopedCollectionResolvable, ScopedCollectionResolvableDecorator>(EnLifestyle.Scoped);
                });

                var overrides = fixture.DelegateOverride(container =>
                {
                    container.OverrideCollection(
                        Enumerable.Empty<object>(),
                        new[] { (typeof(TOverride), lifestyle) },
                        Enumerable.Empty<(Func<IScopedCollectionResolvable>, EnLifestyle)>());
                });

                return new DependencyContainerOptions()
                    .WithManualRegistrations(registrations)
                    .WithOverrides(overrides)
                    .WithAdditionalOurTypes(additionalOurTypes);
            }

            static DependencyContainerOptions OverrideDecorator<TOverride>(
                ModulesTestFixture fixture,
                Type[] additionalOurTypes,
                EnLifestyle lifestyle)
                where TOverride : IScopedCollectionResolvable, IDecorator<IScopedCollectionResolvable>
            {
                var registrations = fixture.DelegateRegistration(container =>
                {
                    container.RegisterDecorator<IScopedCollectionResolvable, ScopedCollectionResolvableDecorator>(EnLifestyle.Scoped);
                });

                var overrides = fixture.DelegateOverride(container =>
                {
                    container.Override<IScopedCollectionResolvable, TOverride>(lifestyle);
                });

                return new DependencyContainerOptions()
                    .WithManualRegistrations(registrations)
                    .WithOverrides(overrides)
                    .WithAdditionalOurTypes(additionalOurTypes);
            }

            static Func<DependencyContainerOptions, IScopedCollectionResolvable> Resolve(
                ITestOutputHelper output,
                ModulesTestFixture fixture,
                Assembly[] assemblies)
            {
                return options =>
                {
                    var dependencyContainer = fixture.BoundedAboveContainer(output, options, assemblies);

                    using (dependencyContainer.OpenScope())
                    {
                        return dependencyContainer
                            .ResolveCollection<IScopedCollectionResolvable>()
                            .Single();
                    }
                };
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
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(CompositionRoot)))
            };

            var additionalOurTypes = new[]
            {
                typeof(IScopedService),
                typeof(ScopedService),
                typeof(IManuallyRegisteredService),
                typeof(ManuallyRegisteredService),
                typeof(ManuallyRegisteredServiceOverride)
            };

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
                .WithOverrides(overrides)
                .WithAdditionalOurTypes(additionalOurTypes);

            var dependencyContainer = Fixture.BoundedAboveContainer(Output, options, assemblies);
            var resolved = dependencyContainer.Resolve<IManuallyRegisteredService>();

            Assert.NotSame(original, replacement);
            Assert.NotSame(original, resolved);
            Assert.Same(replacement, resolved);
            Assert.Same(resolved, dependencyContainer.Resolve<IManuallyRegisteredService>());
        }

        [Fact]
        internal void OverrideInstanceByInstamceProcuderTest()
        {
            var assemblies = new[]
            {
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(CompositionRoot)))
            };

            var additionalOurTypes = new[]
            {
                typeof(IScopedService),
                typeof(ScopedService),
                typeof(IManuallyRegisteredService),
                typeof(ManuallyRegisteredService),
                typeof(ManuallyRegisteredServiceOverride)
            };

            var original = new ManuallyRegisteredService();

            var registration = Fixture.DelegateRegistration(container =>
            {
                container.RegisterInstance<IManuallyRegisteredService>(original);
                container.RegisterInstance<ManuallyRegisteredService>(original);
            });

            var replacement = new ManuallyRegisteredServiceOverride();

            var wrongOverrides = Fixture.DelegateOverride(container =>
            {
                container.OverrideDelegate<IManuallyRegisteredService>(() => replacement, EnLifestyle.Scoped);
            });

            var wrongOptions = new DependencyContainerOptions()
                .WithManualRegistrations(registration)
                .WithOverrides(wrongOverrides)
                .WithAdditionalOurTypes(additionalOurTypes);

            Assert.Throws<ContainerConfigurationException>(() => Fixture.BoundedAboveContainer(Output, wrongOptions, assemblies));

            var overrides = Fixture.DelegateOverride(container =>
            {
                container.OverrideDelegate<IManuallyRegisteredService>(() => replacement, EnLifestyle.Singleton);
            });

            var options = new DependencyContainerOptions()
                .WithManualRegistrations(registration)
                .WithOverrides(overrides)
                .WithAdditionalOurTypes(additionalOurTypes);

            var dependencyContainer = Fixture.BoundedAboveContainer(Output, options, assemblies);
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
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(CompositionRoot)))
            };

            var additionalOurTypes = new[]
            {
                typeof(IScopedService),
                typeof(ScopedService),
                typeof(IManuallyRegisteredService),
                typeof(ManuallyRegisteredService),
                typeof(ManuallyRegisteredServiceOverride)
            };

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
                .WithOverrides(overrides)
                .WithAdditionalOurTypes(additionalOurTypes);

            var dependencyContainer = Fixture.BoundedAboveContainer(Output, options, assemblies);
            var resolved = dependencyContainer.Resolve<IManuallyRegisteredService>();

            Assert.Equal(typeof(ManuallyRegisteredServiceOverride), resolved.GetType());
            Assert.Same(resolved, dependencyContainer.Resolve<IManuallyRegisteredService>());
        }
    }
}