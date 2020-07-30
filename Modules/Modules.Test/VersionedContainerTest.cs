namespace SpaceEngineers.Core.Modules.Test
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
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
    [SuppressMessage("Coupling", "CA1506", Justification = "For test reasons")]
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

                CheckTransient(ResolveOriginal, ResolveVersions, ResolveVersioned, typeof(TransientVersionedServiceImpl), currentVersion, expectedVersionsTypes);
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

                CheckScoped(ResolveOriginal, ResolveVersions, ResolveVersioned, typeof(ScopedVersionedServiceImpl), currentVersion, expectedVersionsTypes);
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

                CheckSingleton(ResolveOriginal, ResolveVersions, ResolveVersioned, typeof(SingletonVersionedServiceImpl), currentVersion, expectedVersionsTypes);
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
                             () => Check(typeof(TransientVersionedServiceImpl),
                                         typeof(ScopedVersionedServiceImpl),
                                         typeof(SingletonVersionedServiceImpl),
                                         typeof(TransientImplementation),
                                         typeof(ScopedImplementation),
                                         typeof(SingletonImplementation)),
                             () => Check(typeof(TransientVersionedServiceImplV2),
                                         typeof(ScopedVersionedServiceImplV2),
                                         typeof(SingletonVersionedServiceImplV2),
                                         typeof(TransientImplementationV2),
                                         typeof(ScopedImplementationV2),
                                         typeof(SingletonImplementationV2)),
                             () => Check(typeof(TransientVersionedServiceImplV3),
                                         typeof(ScopedVersionedServiceImplV3),
                                         typeof(SingletonVersionedServiceImplV3),
                                         typeof(TransientImplementationV3),
                                         typeof(ScopedImplementationV3),
                                         typeof(SingletonImplementationV3)));
            }

            IDisposable ApplyFirst()
            {
                return new CompositeDisposable(DependencyContainer.UseVersion<ITransientVersionedService, TransientVersionedServiceImplV2>(),
                                               DependencyContainer.UseVersion<IScopedVersionedService, ScopedVersionedServiceImplV2>(),
                                               DependencyContainer.UseVersion<ISingletonVersionedService, SingletonVersionedServiceImplV2>(),
                                               DependencyContainer.UseVersion<TransientImplementation, TransientImplementationV2>(),
                                               DependencyContainer.UseVersion<ScopedImplementation, ScopedImplementationV2>(),
                                               DependencyContainer.UseVersion<SingletonImplementation, SingletonImplementationV2>());
            }

            IDisposable ApplySecond()
            {
                return new CompositeDisposable(DependencyContainer.UseVersion<ITransientVersionedService, TransientVersionedServiceImplV3>(),
                                               DependencyContainer.UseVersion<IScopedVersionedService, ScopedVersionedServiceImplV3>(),
                                               DependencyContainer.UseVersion<ISingletonVersionedService, SingletonVersionedServiceImplV3>(),
                                               DependencyContainer.UseVersion<TransientImplementation, TransientImplementationV3>(),
                                               DependencyContainer.UseVersion<ScopedImplementation, ScopedImplementationV3>(),
                                               DependencyContainer.UseVersion<SingletonImplementation, SingletonImplementationV3>());
            }

            void Check(Type currentTransientVersion,
                       Type currentScopedVersion,
                       Type currentSingletonVersion,
                       Type currentTransientImplementationVersion,
                       Type currentScopedImplementationVersion,
                       Type currentSingletonImplementationVersion)
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

                var expectedTransientImplementationVersionsTypes = new[]
                                                                   {
                                                                       typeof(TransientImplementationV2),
                                                                       typeof(TransientImplementationV3),
                                                                   };

                var expectedScopedImplementationVersionsTypes = new[]
                                                                {
                                                                    typeof(ScopedImplementationV2),
                                                                    typeof(ScopedImplementationV3),
                                                                };

                var expectedSingletonImplementationVersionsTypes = new[]
                                                                   {
                                                                       typeof(SingletonImplementationV2),
                                                                       typeof(SingletonImplementationV3),
                                                                   };

                CheckTransient<ITransientVersionedService>(() => Resolve().Transient.Original, () => Resolve().Transient.Versions, () => Resolve().Transient, typeof(TransientVersionedServiceImpl), currentTransientVersion, expectedTransientVersionsTypes);
                CheckScoped<IScopedVersionedService>(() => Resolve().Scoped.Original, () => Resolve().Scoped.Versions, () => Resolve().Scoped, typeof(ScopedVersionedServiceImpl), currentScopedVersion, expectedScopedVersionsTypes);
                CheckSingleton<ISingletonVersionedService>(() => Resolve().Singleton.Original, () => Resolve().Singleton.Versions, () => Resolve().Singleton, typeof(SingletonVersionedServiceImpl), currentSingletonVersion, expectedSingletonVersionsTypes);
                CheckTransient<TransientImplementation>(() => Resolve().TransientImplementation.Original, () => Resolve().TransientImplementation.Versions, () => Resolve().TransientImplementation, typeof(TransientImplementation), currentTransientImplementationVersion, expectedTransientImplementationVersionsTypes);
                CheckScoped<ScopedImplementation>(() => Resolve().ScopedImplementation.Original, () => Resolve().ScopedImplementation.Versions, () => Resolve().ScopedImplementation, typeof(ScopedImplementation), currentScopedImplementationVersion, expectedScopedImplementationVersionsTypes);
                CheckSingleton<SingletonImplementation>(() => Resolve().SingletonImplementation.Original, () => Resolve().SingletonImplementation.Versions, () => Resolve().SingletonImplementation, typeof(SingletonImplementation), currentSingletonImplementationVersion, expectedSingletonImplementationVersionsTypes);
            }

            IServiceWithVersionedDependencies Resolve() => DependencyContainer.Resolve<IServiceWithVersionedDependencies>();
        }

        [Fact]
        internal void TransientImplementationTest()
        {
            VerifyNested(ApplyFirst,
                         ApplySecond,
                         () => Check(typeof(TransientImplementation)),
                         () => Check(typeof(TransientImplementationV2)),
                         () => Check(typeof(TransientImplementationV3)));

            IDisposable ApplyFirst() => DependencyContainer.UseVersion<TransientImplementation, TransientImplementationV2>();
            IDisposable ApplySecond() => DependencyContainer.UseVersion<TransientImplementation, TransientImplementationV3>();

            void Check(Type currentVersion)
            {
                var expectedVersionsTypes = new[]
                                         {
                                             typeof(TransientImplementationV2),
                                             typeof(TransientImplementationV3)
                                         };

                CheckTransient(ResolveOriginal, ResolveVersions, ResolveVersioned, typeof(TransientImplementation), currentVersion, expectedVersionsTypes);
            }

            TransientImplementation ResolveOriginal() => DependencyContainer.Resolve<TransientImplementation>();
            IEnumerable<TransientImplementation> ResolveVersions() => DependencyContainer.ResolveCollection<IVersionFor<TransientImplementation>>().Select(z => z.Version);
            IVersioned<TransientImplementation> ResolveVersioned() => DependencyContainer.Resolve<IVersioned<TransientImplementation>>();
        }

        [Fact]
        internal void ScopedImplementationTest()
        {
            using (DependencyContainer.OpenScope())
            {
                VerifyNested(ApplyFirst,
                             ApplySecond,
                             () => Check(typeof(ScopedImplementation)),
                             () => Check(typeof(ScopedImplementationV2)),
                             () => Check(typeof(ScopedImplementationV3)));
            }

            IDisposable ApplyFirst() => DependencyContainer.UseVersion<ScopedImplementation, ScopedImplementationV2>();
            IDisposable ApplySecond() => DependencyContainer.UseVersion<ScopedImplementation, ScopedImplementationV3>();

            void Check(Type currentVersion)
            {
                var expectedVersionsTypes = new[]
                                         {
                                             typeof(ScopedImplementationV2),
                                             typeof(ScopedImplementationV3)
                                         };

                CheckScoped(ResolveOriginal, ResolveVersions, ResolveVersioned, typeof(ScopedImplementation), currentVersion, expectedVersionsTypes);
            }

            ScopedImplementation ResolveOriginal() => DependencyContainer.Resolve<ScopedImplementation>();
            IEnumerable<ScopedImplementation> ResolveVersions() => DependencyContainer.ResolveCollection<IVersionFor<ScopedImplementation>>().Select(z => z.Version);
            IVersioned<ScopedImplementation> ResolveVersioned() => DependencyContainer.Resolve<IVersioned<ScopedImplementation>>();
        }

        [Fact]
        internal void SingletonImplementationTest()
        {
            using (DependencyContainer.OpenScope())
            {
                VerifyNested(ApplyFirst,
                             ApplySecond,
                             () => Check(typeof(SingletonImplementation)),
                             () => Check(typeof(SingletonImplementationV2)),
                             () => Check(typeof(SingletonImplementationV3)));
            }

            IDisposable ApplyFirst() => DependencyContainer.UseVersion<SingletonImplementation, SingletonImplementationV2>();
            IDisposable ApplySecond() => DependencyContainer.UseVersion<SingletonImplementation, SingletonImplementationV3>();

            void Check(Type currentVersion)
            {
                var expectedVersionsTypes = new[]
                                         {
                                             typeof(SingletonImplementationV2),
                                             typeof(SingletonImplementationV3)
                                         };

                CheckSingleton(ResolveOriginal, ResolveVersions, ResolveVersioned, typeof(SingletonImplementation), currentVersion, expectedVersionsTypes);
            }

            SingletonImplementation ResolveOriginal() => DependencyContainer.Resolve<SingletonImplementation>();
            IEnumerable<SingletonImplementation> ResolveVersions() => DependencyContainer.ResolveCollection<IVersionFor<SingletonImplementation>>().Select(z => z.Version);
            IVersioned<SingletonImplementation> ResolveVersioned() => DependencyContainer.Resolve<IVersioned<SingletonImplementation>>();
        }

        [Fact]
        internal void DecoratedServiceTest()
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
        internal void DecoratedImplementationTest()
        {
            throw new NotImplementedException();
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

        private void CheckTransient<TTransient>(Func<TTransient> resolveOriginal,
                                                Func<IEnumerable<TTransient>> resolveVersions,
                                                Func<IVersioned<TTransient>> resolveVersioned,
                                                Type originalVersion,
                                                Type currentVersion,
                                                Type[] expectedVersionsTypes)
            where TTransient : class
        {
            // original
            Assert.Equal(originalVersion, resolveOriginal().GetType());
            Assert.NotSame(resolveOriginal(), resolveOriginal());
            Assert.Equal(originalVersion, resolveVersioned().Original.GetType());
            Assert.NotSame(resolveVersioned().Original, resolveVersioned().Original);

            // versions
            Assert.True(expectedVersionsTypes.SequenceEqual(Types(resolveVersions())));
            Assert.True(expectedVersionsTypes.SequenceEqual(Types(resolveVersioned().Versions)));

            // versioned
            Assert.True(resolveVersioned() is Versioned<TTransient>);
            Assert.NotSame(resolveVersioned(), resolveVersioned());
            Assert.Equal(currentVersion, resolveVersioned().Current.GetType());
            Assert.NotSame(resolveVersioned().Current, resolveVersioned().Current);
        }

        private void CheckScoped<TScoped>(Func<TScoped> resolveOriginal,
                                          Func<IEnumerable<TScoped>> resolveVersions,
                                          Func<IVersioned<TScoped>> resolveVersioned,
                                          Type originalVersion,
                                          Type currentVersion,
                                          Type[] expectedVersionsTypes)
            where TScoped : class
        {
            // original
            Assert.Equal(originalVersion, resolveOriginal().GetType());
            Assert.Same(resolveOriginal(), resolveOriginal());
            var outer = resolveOriginal();
            using (DependencyContainer.OpenScope())
            {
                Assert.NotSame(outer, resolveOriginal());
            }

            Assert.Equal(originalVersion, resolveVersioned().Original.GetType());
            Assert.Same(resolveVersioned().Original, resolveVersioned().Original);
            outer = resolveVersioned().Original;
            using (DependencyContainer.OpenScope())
            {
                Assert.NotSame(outer, resolveVersioned().Original);
            }

            // versions
            Assert.True(expectedVersionsTypes.SequenceEqual(Types(resolveVersions())));
            Assert.True(expectedVersionsTypes.SequenceEqual(Types(resolveVersioned().Versions)));

            // versioned
            Assert.True(resolveVersioned() is Versioned<TScoped>);
            Assert.Same(resolveVersioned(), resolveVersioned());
            var outerVersioned = resolveVersioned();
            using (DependencyContainer.OpenScope())
            {
                Assert.NotSame(outerVersioned, resolveVersioned());
            }

            Assert.Equal(currentVersion, resolveVersioned().Current.GetType());
            Assert.Same(resolveVersioned().Current, resolveVersioned().Current);
            outer = resolveVersioned().Current;
            using (DependencyContainer.OpenScope())
            {
                Assert.NotSame(outer, resolveVersioned().Current);
            }
        }

        private void CheckSingleton<TSingleton>(Func<TSingleton> resolveOriginal,
                                                Func<IEnumerable<TSingleton>> resolveVersions,
                                                Func<IVersioned<TSingleton>> resolveVersioned,
                                                Type originalVersion,
                                                Type currentVersion,
                                                Type[] expectedVersionsTypes)
            where TSingleton : class
        {
            // original
            Assert.Equal(originalVersion, resolveOriginal().GetType());
            Assert.Same(resolveOriginal(), resolveOriginal());
            Assert.Equal(originalVersion, resolveVersioned().Original.GetType());
            Assert.Same(resolveVersioned().Original, resolveVersioned().Original);

            // versions
            Assert.True(expectedVersionsTypes.SequenceEqual(Types(resolveVersions())));
            Assert.True(expectedVersionsTypes.SequenceEqual(Types(resolveVersioned().Versions)));

            // versioned
            Assert.True(resolveVersioned() is Versioned<TSingleton>);
            Assert.Same(resolveVersioned(), resolveVersioned());
            Assert.Equal(currentVersion, resolveVersioned().Current.GetType());
            Assert.Same(resolveVersioned().Current, resolveVersioned().Current);
        }
    }
}