namespace SpaceEngineers.Core.Modules.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Disposables;
    using AutoRegistration;
    using AutoRegistration.Internals;
    using AutoWiringApi.Abstractions;
    using VersionedContainer;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// IVersionedContainer class tests
    /// </summary>
    public class VersionedContainerTest : ModulesTestBase
    {
        /// <summary> .ctor </summary>
        /// <param name="output">ITestOutputHelper</param>
        public VersionedContainerTest(ITestOutputHelper output)
            : base(output) { }

        [Fact]
        internal void TransientVersionsTest()
        {
            VerifyNested(ApplyFirst,
                         ApplySecond,
                         () => Check(typeof(TransientVersionedServiceImpl)),
                         () => Check(typeof(TransientVersionedServiceImplV2)),
                         () => Check(typeof(TransientVersionedServiceImplV3)));

            IDisposable ApplyFirst() => DependencyContainer.UseVersion<ITransientVersionedService, TransientVersionedServiceImplV2>();
            IDisposable ApplySecond() => DependencyContainer.UseVersion<ITransientVersionedService, TransientVersionedServiceImplV3>();

            void Check(Type currentVersion)
            {
                var expectedVersionsTypes = new[]
                                            {
                                                typeof(TransientVersionedServiceImplV2),
                                                typeof(TransientVersionedServiceImplV3)
                                            };

                CheckTransient(ResolveOriginal, ResolveVersions, ResolveVersioned, currentVersion, expectedVersionsTypes);
            }

            ITransientVersionedService ResolveOriginal() => DependencyContainer.Resolve<ITransientVersionedService>();
            IEnumerable<ITransientVersionedService> ResolveVersions() => DependencyContainer.ResolveCollection<IVersionFor<ITransientVersionedService>>().Select(z => z.Version);
            IVersioned<ITransientVersionedService> ResolveVersioned() => DependencyContainer.Resolve<IVersioned<ITransientVersionedService>>();
        }

        [Fact]
        internal void ScopedVersionsTest()
        {
            using (DependencyContainer.OpenScope())
            {
                VerifyNested(ApplyFirst,
                             ApplySecond,
                             () => Check(typeof(ScopedVersionedServiceImpl)),
                             () => Check(typeof(ScopedVersionedServiceImplV2)),
                             () => Check(typeof(ScopedVersionedServiceImplV3)));
            }

            IDisposable ApplyFirst() => DependencyContainer.UseVersion<IScopedVersionedService, ScopedVersionedServiceImplV2>();
            IDisposable ApplySecond() => DependencyContainer.UseVersion<IScopedVersionedService, ScopedVersionedServiceImplV3>();

            void Check(Type currentVersion)
            {
                var expectedVersionsTypes = new[]
                                            {
                                                typeof(ScopedVersionedServiceImplV2),
                                                typeof(ScopedVersionedServiceImplV3)
                                            };

                CheckScoped(ResolveOriginal, ResolveVersions, ResolveVersioned, currentVersion, expectedVersionsTypes);
            }

            IScopedVersionedService ResolveOriginal() => DependencyContainer.Resolve<IScopedVersionedService>();
            IEnumerable<IScopedVersionedService> ResolveVersions() => DependencyContainer.ResolveCollection<IVersionFor<IScopedVersionedService>>().Select(z => z.Version);
            IVersioned<IScopedVersionedService> ResolveVersioned() => DependencyContainer.Resolve<IVersioned<IScopedVersionedService>>();
        }

        [Fact]
        internal void SingletonVersionsTest()
        {
            VerifyNested(ApplyFirst,
                         ApplySecond,
                         () => Check(typeof(SingletonVersionedServiceImpl)),
                         () => Check(typeof(SingletonVersionedServiceImplV2)),
                         () => Check(typeof(SingletonVersionedServiceImplV3)));

            IDisposable ApplyFirst() => DependencyContainer.UseVersion<ISingletonVersionedService, SingletonVersionedServiceImplV2>();
            IDisposable ApplySecond() => DependencyContainer.UseVersion<ISingletonVersionedService, SingletonVersionedServiceImplV3>();

            void Check(Type currentVersion)
            {
                var expectedVersionsTypes = new[]
                                            {
                                                typeof(SingletonVersionedServiceImplV2),
                                                typeof(SingletonVersionedServiceImplV3)
                                            };

                CheckSingleton(ResolveOriginal, ResolveVersions, ResolveVersioned, currentVersion, expectedVersionsTypes);
            }

            ISingletonVersionedService ResolveOriginal() => DependencyContainer.Resolve<ISingletonVersionedService>();
            IEnumerable<ISingletonVersionedService> ResolveVersions() => DependencyContainer.ResolveCollection<IVersionFor<ISingletonVersionedService>>().Select(z => z.Version);
            IVersioned<ISingletonVersionedService> ResolveVersioned() => DependencyContainer.Resolve<IVersioned<ISingletonVersionedService>>();
        }

        [Fact]
        internal void CompositeVersionsTest()
        {
            using (DependencyContainer.OpenScope())
            {
                VerifyNested(ApplyFirst,
                             ApplySecond,
                             () => Check(typeof(TransientVersionedServiceImpl), typeof(ScopedVersionedServiceImpl), typeof(SingletonVersionedServiceImpl)),
                             () => Check(typeof(TransientVersionedServiceImplV2), typeof(ScopedVersionedServiceImplV2), typeof(SingletonVersionedServiceImplV2)),
                             () => Check(typeof(TransientVersionedServiceImplV3), typeof(ScopedVersionedServiceImplV3), typeof(SingletonVersionedServiceImplV3)));
            }

            IDisposable ApplyFirst()
            {
                return new CompositeDisposable(DependencyContainer.UseVersion<ITransientVersionedService, TransientVersionedServiceImplV2>(),
                                               DependencyContainer.UseVersion<IScopedVersionedService, ScopedVersionedServiceImplV2>(),
                                               DependencyContainer.UseVersion<ISingletonVersionedService, SingletonVersionedServiceImplV2>());
            }

            IDisposable ApplySecond()
            {
                return new CompositeDisposable(DependencyContainer.UseVersion<ITransientVersionedService, TransientVersionedServiceImplV3>(),
                                               DependencyContainer.UseVersion<IScopedVersionedService, ScopedVersionedServiceImplV3>(),
                                               DependencyContainer.UseVersion<ISingletonVersionedService, SingletonVersionedServiceImplV3>());
            }

            void Check(Type currentTransientVersion, Type currentScopedVersion, Type currentSingletonVersion)
            {
                var expectedTransientVersionsTypes = new[]
                                                     {
                                                         typeof(TransientVersionedServiceImplV2),
                                                         typeof(TransientVersionedServiceImplV3)
                                                     };

                var expectedScopedVersionsTypes = new[]
                                                  {
                                                      typeof(ScopedVersionedServiceImplV2),
                                                      typeof(ScopedVersionedServiceImplV3)
                                                  };

                var expectedSingletonVersionsTypes = new[]
                                                     {
                                                         typeof(SingletonVersionedServiceImplV2),
                                                         typeof(SingletonVersionedServiceImplV3)
                                                     };

                CheckTransient(() => Resolve().Transient.Original, () => Resolve().Transient.Versions, () => Resolve().Transient, currentTransientVersion, expectedTransientVersionsTypes);
                CheckScoped(() => Resolve().Scoped.Original, () => Resolve().Scoped.Versions, () => Resolve().Scoped, currentScopedVersion, expectedScopedVersionsTypes);
                CheckSingleton(() => Resolve().Singleton.Original, () => Resolve().Singleton.Versions, () => Resolve().Singleton, currentSingletonVersion, expectedSingletonVersionsTypes);
            }

            IServiceWithVersionedDependencies Resolve() => DependencyContainer.Resolve<IServiceWithVersionedDependencies>();
        }

        [Fact]
        internal void ImplementationVersionsTest()
        {
            throw new NotImplementedException();
        }

        [Fact]
        internal void DecoratedVersionTest()
        {
            var outer = new[]
                        {
                            typeof(VersionedAndDecoratedDecorator),
                            typeof(VersionedAndDecoratedImpl),
                        };

            var first = new[]
                        {
                            typeof(VersionedAndDecoratedDecorator),
                            typeof(VersionedAndDecoratedImplV2),
                        };

            var second = new[]
                         {
                             typeof(VersionedAndDecoratedDecorator),
                             typeof(VersionedAndDecoratedImplV3),
                         };

            var versions = new[]
                           {
                               typeof(VersionedAndDecoratedImplV2),
                               typeof(VersionedAndDecoratedImplV3),
                           };

            VerifyNested(ApplyFirst,
                         ApplySecond,
                         () => Check(outer, outer, versions),
                         () => Check(outer, first, versions),
                         () => Check(outer, second, versions));

            IDisposable ApplyFirst() => DependencyContainer.UseVersion<IVersionedAndDecorated, VersionedAndDecoratedImplV2>();
            IDisposable ApplySecond() => DependencyContainer.UseVersion<IVersionedAndDecorated, VersionedAndDecoratedImplV3>();

            void Check(ICollection<Type> originalStructure, ICollection<Type> structure, ICollection<Type> resolvedVersions)
            {
                Assert.True(originalStructure.SequenceEqual(ExtractDecorators(ResolveOriginal())));
                Assert.True(originalStructure.SequenceEqual(ExtractDecorators(ResolveVersioned().Original)));
                Assert.True(structure.SequenceEqual(ExtractDecorators(ResolveVersioned().Current)));
                Assert.True(resolvedVersions.SequenceEqual(Types(ResolveVersions())));
                Assert.True(resolvedVersions.SequenceEqual(Types(ResolveVersioned().Versions)));
            }

            IVersionedAndDecorated ResolveOriginal() => DependencyContainer.Resolve<IVersionedAndDecorated>();
            IEnumerable<IVersionedAndDecorated> ResolveVersions() => DependencyContainer.ResolveCollection<IVersionFor<IVersionedAndDecorated>>().Select(z => z.Version);
            IVersioned<IVersionedAndDecorated> ResolveVersioned() => DependencyContainer.Resolve<IVersioned<IVersionedAndDecorated>>();

            IEnumerable<Type> ExtractDecorators(IVersionedAndDecorated service)
            {
                while (service is IVersionedAndDecoratedDecorator decorator)
                {
                    yield return decorator.GetType();
                    service = decorator.Decoratee;
                }

                yield return service.GetType();
            }
        }

        [Fact]
        internal void CollectionVersionsTest()
        {
            throw new NotImplementedException();
        }

        private static void VerifyNested(Func<IDisposable> applyFirst,
                                         Func<IDisposable> applySecond,
                                         Action outer,
                                         Action first,
                                         Action second)
        {
            outer();

            using (applyFirst())
            {
                first();

                using (applySecond())
                {
                    second();
                }

                first();
            }

            outer();
        }

        private static IEnumerable<Type> Types(IEnumerable<object> objects)
        {
            return objects.Select(obj => obj.GetType());
        }

        private void CheckTransient(Func<ITransientVersionedService> resolveOriginal,
                                    Func<IEnumerable<ITransientVersionedService>> resolveVersions,
                                    Func<IVersioned<ITransientVersionedService>> resolveVersioned,
                                    Type currentVersion,
                                    Type[] expectedVersionsTypes)
        {
            // original
            Assert.True(resolveOriginal() is TransientVersionedServiceImpl);
            Assert.NotSame(resolveOriginal(), resolveOriginal());

            // versions
            Assert.True(expectedVersionsTypes.SequenceEqual(Types(resolveVersions())));

            // versioned
            Assert.True(resolveVersioned() is Versioned<ITransientVersionedService>);
            Assert.NotSame(resolveVersioned(), resolveVersioned());

            Assert.True(resolveVersioned().Original is TransientVersionedServiceImpl);
            Assert.NotSame(resolveVersioned().Original, resolveVersioned().Original);
            Assert.Equal(currentVersion, resolveVersioned().Current.GetType());
            Assert.NotSame(resolveVersioned().Current, resolveVersioned().Current);

            Assert.True(expectedVersionsTypes.SequenceEqual(Types(resolveVersioned().Versions)));
        }

        private void CheckScoped(Func<IScopedVersionedService> resolveOriginal,
                                 Func<IEnumerable<IScopedVersionedService>> resolveVersions,
                                 Func<IVersioned<IScopedVersionedService>> resolveVersioned,
                                 Type currentVersion,
                                 Type[] expectedVersionsTypes)
        {
            // original
            Assert.True(resolveOriginal() is ScopedVersionedServiceImpl);
            Assert.Same(resolveOriginal(), resolveOriginal());
            var outer = resolveOriginal();
            using (DependencyContainer.OpenScope())
            {
                Assert.NotSame(outer, resolveOriginal());
            }

            // versions
            Assert.True(expectedVersionsTypes.SequenceEqual(Types(resolveVersions())));

            // versioned
            Assert.True(resolveVersioned() is Versioned<IScopedVersionedService>);
            Assert.Same(resolveVersioned(), resolveVersioned());
            var outerVersioned = resolveVersioned();
            using (DependencyContainer.OpenScope())
            {
                Assert.NotSame(outerVersioned, resolveVersioned());
            }

            Assert.True(resolveVersioned().Original is ScopedVersionedServiceImpl);
            Assert.Same(resolveVersioned().Original, resolveVersioned().Original);
            outer = resolveVersioned().Original;
            using (DependencyContainer.OpenScope())
            {
                Assert.NotSame(outer, resolveVersioned().Original);
            }

            Assert.Equal(currentVersion, resolveVersioned().Current.GetType());
            Assert.Same(resolveVersioned().Current, resolveVersioned().Current);
            outer = resolveVersioned().Current;
            using (DependencyContainer.OpenScope())
            {
                Assert.NotSame(outer, resolveVersioned().Current);
            }

            Assert.True(expectedVersionsTypes.SequenceEqual(Types(resolveVersioned().Versions)));
        }

        private void CheckSingleton(Func<ISingletonVersionedService> resolveOriginal,
                                    Func<IEnumerable<ISingletonVersionedService>> resolveVersions,
                                    Func<IVersioned<ISingletonVersionedService>> resolveVersioned,
                                    Type currentVersion,
                                    Type[] expectedVersionsTypes)
        {
            // original
            Assert.True(resolveOriginal() is SingletonVersionedServiceImpl);
            Assert.Same(resolveOriginal(), resolveOriginal());

            // versions
            Assert.True(expectedVersionsTypes.SequenceEqual(Types(resolveVersions())));

            // versioned
            Assert.True(resolveVersioned() is Versioned<ISingletonVersionedService>);
            Assert.Same(resolveVersioned(), resolveVersioned());

            Assert.True(resolveVersioned().Original is SingletonVersionedServiceImpl);
            Assert.Same(resolveVersioned().Original, resolveVersioned().Original);
            Assert.Equal(currentVersion, resolveVersioned().Current.GetType());
            Assert.Same(resolveVersioned().Current, resolveVersioned().Current);

            Assert.True(expectedVersionsTypes.SequenceEqual(Types(resolveVersioned().Versions)));
        }
    }
}