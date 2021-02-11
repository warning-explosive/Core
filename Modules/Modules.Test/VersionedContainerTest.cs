namespace SpaceEngineers.Core.Modules.Test
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using AutoRegistration.Abstractions;
    using AutoWiringApi.Abstractions;
    using Basics;
    using Basics.Test;
    using ClassFixtures;
    using VersionedContainer;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// IVersionedContainer class tests
    /// </summary>
    [SuppressMessage("Coupling", "CA1506", Justification = "For test reasons")]
    public class VersionedContainerTest : BasicsTestBase, IClassFixture<ModulesTestFixture>
    {
        /// <summary> .ctor </summary>
        /// <param name="output">ITestOutputHelper</param>
        /// <param name="fixture">ModulesTestFixture</param>
        public VersionedContainerTest(ITestOutputHelper output, ModulesTestFixture fixture)
            : base(output)
        {
            DependencyContainer = fixture.DefaultDependencyContainer;
        }

        /// <summary>
        /// DependencyContainer
        /// </summary>
        protected IDependencyContainer DependencyContainer { get; }

        [Fact]
        internal void DependencyWithoutVersionsTest()
        {
            var versioned = DependencyContainer.Resolve<ImplementationWithDependencyWithoutVersions>().Versioned;

            Assert.Empty(versioned.Versions);
        }

        [Fact]
        internal void TransientVersionsTest()
        {
            TransientVersionsTestInternal(
                () => DependencyContainer.UseVersion<ITransientVersionedService, TransientVersionedServiceImplV2>(),
                () => DependencyContainer.UseVersion<ITransientVersionedService, TransientVersionedServiceImplV3>());

            TransientVersionsTestInternal(
                () => DependencyContainer.UseVersion<ITransientVersionedService>(() => new TransientVersionedServiceImplV2()),
                () => DependencyContainer.UseVersion<ITransientVersionedService>(() => new TransientVersionedServiceImplV3()));
        }

        internal void TransientVersionsTestInternal(Func<IDisposable> applyFirst, Func<IDisposable> applySecond)
        {
            var original = new[]
                           {
                               typeof(TransientVersionedServiceDecorator),
                               typeof(TransientVersionedServiceImpl),
                           };

            var first = new[]
                        {
                            typeof(TransientVersionedServiceDecorator),
                            typeof(TransientVersionedServiceImplV2),
                        };

            var second = new[]
                         {
                             typeof(TransientVersionedServiceDecorator),
                             typeof(TransientVersionedServiceImplV3),
                         };

            VerifyNested(applyFirst,
                         applySecond,
                         () => Check(typeof(TransientVersionedServiceImpl), original, original),
                         () => Check(typeof(TransientVersionedServiceImplV2), original, first),
                         () => Check(typeof(TransientVersionedServiceImplV3), original, second));

            void Check(Type currentVersion,
                       ICollection<Type> originalStructure,
                       ICollection<Type> currentStructure)
            {
                var expectedVersionsTypes = new[]
                                            {
                                                typeof(TransientVersionedServiceImplV2),
                                                typeof(TransientVersionedServiceImplV3)
                                            };

                CheckTransient(ResolveOriginal, ResolveVersions, ResolveVersioned, typeof(TransientVersionedServiceImpl), currentVersion, expectedVersionsTypes);
                CheckDecorators(ResolveOriginal, ResolveVersions, ResolveVersioned, originalStructure, currentStructure, expectedVersionsTypes);
            }

            ITransientVersionedService ResolveOriginal() => DependencyContainer.Resolve<ITransientVersionedService>();
            IEnumerable<ITransientVersionedService> ResolveVersions() => DependencyContainer.ResolveCollection<IVersionFor<ITransientVersionedService>>().Select(z => z.Version);
            IVersioned<ITransientVersionedService> ResolveVersioned() => DependencyContainer.Resolve<IVersioned<ITransientVersionedService>>();
        }

        [Fact]
        internal void ScopedVersionsTest()
        {
            ScopedVersionsTestInternal(
                () => DependencyContainer.UseVersion<IScopedVersionedService, ScopedVersionedServiceImplV2>(),
                () => DependencyContainer.UseVersion<IScopedVersionedService, ScopedVersionedServiceImplV3>());

            ScopedVersionsTestInternal(
                () => DependencyContainer.UseVersion<IScopedVersionedService>(() => new ScopedVersionedServiceImplV2()),
                () => DependencyContainer.UseVersion<IScopedVersionedService>(() => new ScopedVersionedServiceImplV3()));
        }

        internal void ScopedVersionsTestInternal(Func<IDisposable> applyFirst, Func<IDisposable> applySecond)
        {
            var original = new[]
                           {
                               typeof(ScopedVersionedServiceDecorator),
                               typeof(ScopedVersionedServiceImpl),
                           };

            var first = new[]
                        {
                            typeof(ScopedVersionedServiceDecorator),
                            typeof(ScopedVersionedServiceImplV2),
                        };

            var second = new[]
                         {
                             typeof(ScopedVersionedServiceDecorator),
                             typeof(ScopedVersionedServiceImplV3),
                         };

            using (DependencyContainer.OpenScope())
            {
                VerifyNested(applyFirst,
                             applySecond,
                             () => Check(typeof(ScopedVersionedServiceImpl), original, original),
                             () => Check(typeof(ScopedVersionedServiceImplV2), original, first),
                             () => Check(typeof(ScopedVersionedServiceImplV3), original, second));
            }

            void Check(Type currentVersion,
                       ICollection<Type> originalStructure,
                       ICollection<Type> currentStructure)
            {
                var expectedVersionsTypes = new[]
                                            {
                                                typeof(ScopedVersionedServiceImplV2),
                                                typeof(ScopedVersionedServiceImplV3)
                                            };

                CheckScoped(ResolveOriginal, ResolveVersions, ResolveVersioned, typeof(ScopedVersionedServiceImpl), currentVersion, expectedVersionsTypes);
                CheckDecorators(ResolveOriginal, ResolveVersions, ResolveVersioned, originalStructure, currentStructure, expectedVersionsTypes);
            }

            IScopedVersionedService ResolveOriginal() => DependencyContainer.Resolve<IScopedVersionedService>();
            IEnumerable<IScopedVersionedService> ResolveVersions() => DependencyContainer.ResolveCollection<IVersionFor<IScopedVersionedService>>().Select(z => z.Version);
            IVersioned<IScopedVersionedService> ResolveVersioned() => DependencyContainer.Resolve<IVersioned<IScopedVersionedService>>();
        }

        [Fact]
        internal void SingletonVersionsTest()
        {
            SingletonVersionsTestInternal(
                () => DependencyContainer.UseVersion<ISingletonVersionedService, SingletonVersionedServiceImplV2>(),
                () => DependencyContainer.UseVersion<ISingletonVersionedService, SingletonVersionedServiceImplV3>());

            SingletonVersionsTestInternal(
                () => DependencyContainer.UseVersion<ISingletonVersionedService>(() => new SingletonVersionedServiceImplV2()),
                () => DependencyContainer.UseVersion<ISingletonVersionedService>(() => new SingletonVersionedServiceImplV3()));
        }

        internal void SingletonVersionsTestInternal(Func<IDisposable> applyFirst, Func<IDisposable> applySecond)
        {
            var original = new[]
                           {
                               typeof(SingletonVersionedServiceDecorator),
                               typeof(SingletonVersionedServiceImpl),
                           };

            var first = new[]
                        {
                            typeof(SingletonVersionedServiceDecorator),
                            typeof(SingletonVersionedServiceImplV2),
                        };

            var second = new[]
                         {
                             typeof(SingletonVersionedServiceDecorator),
                             typeof(SingletonVersionedServiceImplV3),
                         };

            VerifyNested(applyFirst,
                         applySecond,
                         () => Check(typeof(SingletonVersionedServiceImpl), original, original),
                         () => Check(typeof(SingletonVersionedServiceImplV2), original, first),
                         () => Check(typeof(SingletonVersionedServiceImplV3), original, second));

            void Check(Type currentVersion,
                       ICollection<Type> originalStructure,
                       ICollection<Type> currentStructure)
            {
                var expectedVersionsTypes = new[]
                                            {
                                                typeof(SingletonVersionedServiceImplV2),
                                                typeof(SingletonVersionedServiceImplV3)
                                            };

                CheckSingleton(ResolveOriginal, ResolveVersions, ResolveVersioned, typeof(SingletonVersionedServiceImpl), currentVersion, expectedVersionsTypes);
                CheckDecorators(ResolveOriginal, ResolveVersions, ResolveVersioned, originalStructure, currentStructure, expectedVersionsTypes);
            }

            ISingletonVersionedService ResolveOriginal() => DependencyContainer.Resolve<ISingletonVersionedService>();
            IEnumerable<ISingletonVersionedService> ResolveVersions() => DependencyContainer.ResolveCollection<IVersionFor<ISingletonVersionedService>>().Select(z => z.Version);
            IVersioned<ISingletonVersionedService> ResolveVersioned() => DependencyContainer.Resolve<IVersioned<ISingletonVersionedService>>();
        }

        [Fact]
        internal void CompositeVersionsTest()
        {
            IDisposable ApplyFirst()
            {
                return Disposable.CreateComposite(
                    DependencyContainer.UseVersion<ITransientVersionedService, TransientVersionedServiceImplV2>(),
                    DependencyContainer.UseVersion<IScopedVersionedService, ScopedVersionedServiceImplV2>(),
                    DependencyContainer.UseVersion<ISingletonVersionedService, SingletonVersionedServiceImplV2>(),
                    DependencyContainer.UseVersion<TransientImplementation, TransientImplementationV2>(),
                    DependencyContainer.UseVersion<ScopedImplementation, ScopedImplementationV2>(),
                    DependencyContainer.UseVersion<SingletonImplementation, SingletonImplementationV2>());
            }

            IDisposable ApplySecond()
            {
                return Disposable.CreateComposite(
                    DependencyContainer.UseVersion<ITransientVersionedService, TransientVersionedServiceImplV3>(),
                    DependencyContainer.UseVersion<IScopedVersionedService, ScopedVersionedServiceImplV3>(),
                    DependencyContainer.UseVersion<ISingletonVersionedService, SingletonVersionedServiceImplV3>(),
                    DependencyContainer.UseVersion<TransientImplementation, TransientImplementationV3>(),
                    DependencyContainer.UseVersion<ScopedImplementation, ScopedImplementationV3>(),
                    DependencyContainer.UseVersion<SingletonImplementation, SingletonImplementationV3>());
            }

            CompositeVersionsTestInternal(ApplyFirst, ApplySecond);

            IDisposable ApplyViaInstanceFirst()
            {
                return Disposable.CreateComposite(
                    DependencyContainer.UseVersion<ITransientVersionedService>(() => new TransientVersionedServiceImplV2()),
                    DependencyContainer.UseVersion<IScopedVersionedService>(() => new ScopedVersionedServiceImplV2()),
                    DependencyContainer.UseVersion<ISingletonVersionedService>(() => new SingletonVersionedServiceImplV2()),
                    DependencyContainer.UseVersion<TransientImplementation>(() => new TransientImplementationV2()),
                    DependencyContainer.UseVersion<ScopedImplementation>(() => new ScopedImplementationV2()),
                    DependencyContainer.UseVersion<SingletonImplementation>(() => new SingletonImplementationV2()));
            }

            IDisposable ApplyViaInstanceSecond()
            {
                return Disposable.CreateComposite(
                    DependencyContainer.UseVersion<ITransientVersionedService>(() => new TransientVersionedServiceImplV3()),
                    DependencyContainer.UseVersion<IScopedVersionedService>(() => new ScopedVersionedServiceImplV3()),
                    DependencyContainer.UseVersion<ISingletonVersionedService>(() => new SingletonVersionedServiceImplV3()),
                    DependencyContainer.UseVersion<TransientImplementation>(() => new TransientImplementationV3()),
                    DependencyContainer.UseVersion<ScopedImplementation>(() => new ScopedImplementationV3()),
                    DependencyContainer.UseVersion<SingletonImplementation>(() => new SingletonImplementationV3()));
            }

            CompositeVersionsTestInternal(ApplyViaInstanceFirst, ApplyViaInstanceSecond);
        }

        internal void CompositeVersionsTestInternal(Func<IDisposable> applyFirst, Func<IDisposable> applySecond)
        {
            var originalTransientStructure = new[]
                                             {
                                                 typeof(TransientVersionedServiceDecorator),
                                                 typeof(TransientVersionedServiceImpl),
                                             };

            var firstTransientStructure = new[]
                                          {
                                              typeof(TransientVersionedServiceDecorator),
                                              typeof(TransientVersionedServiceImplV2),
                                          };

            var secondTransientStructure = new[]
                                           {
                                               typeof(TransientVersionedServiceDecorator),
                                               typeof(TransientVersionedServiceImplV3),
                                           };

            var originalScopedStructure = new[]
                                          {
                                              typeof(ScopedVersionedServiceDecorator),
                                              typeof(ScopedVersionedServiceImpl),
                                          };

            var firstScopedStructure = new[]
                                       {
                                           typeof(ScopedVersionedServiceDecorator),
                                           typeof(ScopedVersionedServiceImplV2),
                                       };

            var secondScopedStructure = new[]
                                        {
                                            typeof(ScopedVersionedServiceDecorator),
                                            typeof(ScopedVersionedServiceImplV3),
                                        };

            var originalSingletonStructure = new[]
                                             {
                                                 typeof(SingletonVersionedServiceDecorator),
                                                 typeof(SingletonVersionedServiceImpl),
                                             };

            var firstSingletonStructure = new[]
                                          {
                                              typeof(SingletonVersionedServiceDecorator),
                                              typeof(SingletonVersionedServiceImplV2),
                                          };

            var secondSingletonStructure = new[]
                                           {
                                               typeof(SingletonVersionedServiceDecorator),
                                               typeof(SingletonVersionedServiceImplV3),
                                           };

            var originalTransientImplStructure = new[]
                                                 {
                                                     typeof(TransientImplementationDecorator),
                                                     typeof(TransientImplementation),
                                                 };

            var firstTransientImplStructure = new[]
                                              {
                                                  typeof(TransientImplementationDecorator),
                                                  typeof(TransientImplementationV2),
                                              };

            var secondTransientImplStructure = new[]
                                               {
                                                   typeof(TransientImplementationDecorator),
                                                   typeof(TransientImplementationV3),
                                               };

            var originalScopedImplStructure = new[]
                                              {
                                                  typeof(ScopedImplementationDecorator),
                                                  typeof(ScopedImplementation),
                                              };

            var firstScopedImplStructure = new[]
                                           {
                                               typeof(ScopedImplementationDecorator),
                                               typeof(ScopedImplementationV2),
                                           };

            var secondScopedImplStructure = new[]
                                            {
                                                typeof(ScopedImplementationDecorator),
                                                typeof(ScopedImplementationV3),
                                            };

            var originalSingletonImplStructure = new[]
                                                 {
                                                     typeof(SingletonImplementationDecorator),
                                                     typeof(SingletonImplementation),
                                                 };

            var firstSingletonImplStructure = new[]
                                              {
                                                  typeof(SingletonImplementationDecorator),
                                                  typeof(SingletonImplementationV2),
                                              };

            var secondSingletonImplStructure = new[]
                                               {
                                                   typeof(SingletonImplementationDecorator),
                                                   typeof(SingletonImplementationV3),
                                               };

            using (DependencyContainer.OpenScope())
            {
                VerifyNested(applyFirst,
                             applySecond,
                             () => Check(typeof(TransientVersionedServiceImpl),
                                         typeof(ScopedVersionedServiceImpl),
                                         typeof(SingletonVersionedServiceImpl),
                                         typeof(TransientImplementation),
                                         typeof(ScopedImplementation),
                                         typeof(SingletonImplementation),
                                         originalTransientStructure,
                                         originalScopedStructure,
                                         originalSingletonStructure,
                                         originalTransientImplStructure,
                                         originalScopedImplStructure,
                                         originalSingletonImplStructure),
                             () => Check(typeof(TransientVersionedServiceImplV2),
                                         typeof(ScopedVersionedServiceImplV2),
                                         typeof(SingletonVersionedServiceImplV2),
                                         typeof(TransientImplementationV2),
                                         typeof(ScopedImplementationV2),
                                         typeof(SingletonImplementationV2),
                                         firstTransientStructure,
                                         firstScopedStructure,
                                         firstSingletonStructure,
                                         firstTransientImplStructure,
                                         firstScopedImplStructure,
                                         firstSingletonImplStructure),
                             () => Check(typeof(TransientVersionedServiceImplV3),
                                         typeof(ScopedVersionedServiceImplV3),
                                         typeof(SingletonVersionedServiceImplV3),
                                         typeof(TransientImplementationV3),
                                         typeof(ScopedImplementationV3),
                                         typeof(SingletonImplementationV3),
                                         secondTransientStructure,
                                         secondScopedStructure,
                                         secondSingletonStructure,
                                         secondTransientImplStructure,
                                         secondScopedImplStructure,
                                         secondSingletonImplStructure));
            }

            void Check(Type currentTransientVersion,
                       Type currentScopedVersion,
                       Type currentSingletonVersion,
                       Type currentTransientImplementationVersion,
                       Type currentScopedImplementationVersion,
                       Type currentSingletonImplementationVersion,
                       Type[] currentTransientStructure,
                       Type[] currentScopedStructure,
                       Type[] currentSingletonStructure,
                       Type[] currentTransientImplementationStructure,
                       Type[] currentScopedImplementationStructure,
                       Type[] currentSingletonImplementationStructure)
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

                CheckDecorators<ITransientVersionedService>(() => Resolve().Transient.Original, () => Resolve().Transient.Versions, () => Resolve().Transient, originalTransientStructure!, currentTransientStructure, expectedTransientVersionsTypes);
                CheckDecorators<IScopedVersionedService>(() => Resolve().Scoped.Original, () => Resolve().Scoped.Versions, () => Resolve().Scoped, originalScopedStructure!, currentScopedStructure, expectedScopedVersionsTypes);
                CheckDecorators<ISingletonVersionedService>(() => Resolve().Singleton.Original, () => Resolve().Singleton.Versions, () => Resolve().Singleton, originalSingletonStructure!, currentSingletonStructure, expectedSingletonVersionsTypes);
                CheckDecorators<TransientImplementation>(() => Resolve().TransientImplementation.Original, () => Resolve().TransientImplementation.Versions, () => Resolve().TransientImplementation, originalTransientImplStructure!, currentTransientImplementationStructure, expectedTransientImplementationVersionsTypes);
                CheckDecorators<ScopedImplementation>(() => Resolve().ScopedImplementation.Original, () => Resolve().ScopedImplementation.Versions, () => Resolve().ScopedImplementation, originalScopedImplStructure!, currentScopedImplementationStructure, expectedScopedImplementationVersionsTypes);
                CheckDecorators<SingletonImplementation>(() => Resolve().SingletonImplementation.Original, () => Resolve().SingletonImplementation.Versions, () => Resolve().SingletonImplementation, originalSingletonImplStructure!, currentSingletonImplementationStructure, expectedSingletonImplementationVersionsTypes);
            }

            IServiceWithVersionedDependencies Resolve() => DependencyContainer.Resolve<IServiceWithVersionedDependencies>();
        }

        [Fact]
        internal void TransientImplementationTest()
        {
            TransientImplementationTestInternal(
                () => DependencyContainer.UseVersion<TransientImplementation, TransientImplementationV2>(),
                () => DependencyContainer.UseVersion<TransientImplementation, TransientImplementationV3>());

            TransientImplementationTestInternal(
                () => DependencyContainer.UseVersion<TransientImplementation>(() => new TransientImplementationV2()),
                () => DependencyContainer.UseVersion<TransientImplementation>(() => new TransientImplementationV3()));
        }

        internal void TransientImplementationTestInternal(Func<IDisposable> applyFirst, Func<IDisposable> applySecond)
        {
            var original = new[]
                           {
                               typeof(TransientImplementationDecorator),
                               typeof(TransientImplementation),
                           };

            var first = new[]
                        {
                            typeof(TransientImplementationDecorator),
                            typeof(TransientImplementationV2),
                        };

            var second = new[]
                         {
                             typeof(TransientImplementationDecorator),
                             typeof(TransientImplementationV3),
                         };

            VerifyNested(applyFirst,
                         applySecond,
                         () => Check(typeof(TransientImplementation), original, original),
                         () => Check(typeof(TransientImplementationV2), original, first),
                         () => Check(typeof(TransientImplementationV3), original, second));

            void Check(Type currentVersion,
                       ICollection<Type> originalStructure,
                       ICollection<Type> currentStructure)
            {
                var expectedVersionsTypes = new[]
                                         {
                                             typeof(TransientImplementationV2),
                                             typeof(TransientImplementationV3)
                                         };

                CheckTransient(ResolveOriginal, ResolveVersions, ResolveVersioned, typeof(TransientImplementation), currentVersion, expectedVersionsTypes);
                CheckDecorators(ResolveOriginal, ResolveVersions, ResolveVersioned, originalStructure, currentStructure, expectedVersionsTypes);
            }

            TransientImplementation ResolveOriginal() => DependencyContainer.Resolve<TransientImplementation>();
            IEnumerable<TransientImplementation> ResolveVersions() => DependencyContainer.ResolveCollection<IVersionFor<TransientImplementation>>().Select(z => z.Version);
            IVersioned<TransientImplementation> ResolveVersioned() => DependencyContainer.Resolve<IVersioned<TransientImplementation>>();
        }

        [Fact]
        internal void ScopedImplementationTest()
        {
            ScopedImplementationTestInternal(
                () => DependencyContainer.UseVersion<ScopedImplementation, ScopedImplementationV2>(),
                () => DependencyContainer.UseVersion<ScopedImplementation, ScopedImplementationV3>());

            ScopedImplementationTestInternal(
                () => DependencyContainer.UseVersion<ScopedImplementation>(() => new ScopedImplementationV2()),
                () => DependencyContainer.UseVersion<ScopedImplementation>(() => new ScopedImplementationV3()));
        }

        internal void ScopedImplementationTestInternal(Func<IDisposable> applyFirst, Func<IDisposable> applySecond)
        {
            var original = new[]
                           {
                               typeof(ScopedImplementationDecorator),
                               typeof(ScopedImplementation),
                           };

            var first = new[]
                        {
                            typeof(ScopedImplementationDecorator),
                            typeof(ScopedImplementationV2),
                        };

            var second = new[]
                         {
                             typeof(ScopedImplementationDecorator),
                             typeof(ScopedImplementationV3),
                         };

            using (DependencyContainer.OpenScope())
            {
                VerifyNested(applyFirst,
                             applySecond,
                             () => Check(typeof(ScopedImplementation), original, original),
                             () => Check(typeof(ScopedImplementationV2), original, first),
                             () => Check(typeof(ScopedImplementationV3), original, second));
            }

            void Check(Type currentVersion,
                       ICollection<Type> originalStructure,
                       ICollection<Type> currentStructure)
            {
                var expectedVersionsTypes = new[]
                                         {
                                             typeof(ScopedImplementationV2),
                                             typeof(ScopedImplementationV3)
                                         };

                CheckScoped(ResolveOriginal, ResolveVersions, ResolveVersioned, typeof(ScopedImplementation), currentVersion, expectedVersionsTypes);
                CheckDecorators(ResolveOriginal, ResolveVersions, ResolveVersioned, originalStructure, currentStructure, expectedVersionsTypes);
            }

            ScopedImplementation ResolveOriginal() => DependencyContainer.Resolve<ScopedImplementation>();
            IEnumerable<ScopedImplementation> ResolveVersions() => DependencyContainer.ResolveCollection<IVersionFor<ScopedImplementation>>().Select(z => z.Version);
            IVersioned<ScopedImplementation> ResolveVersioned() => DependencyContainer.Resolve<IVersioned<ScopedImplementation>>();
        }

        [Fact]
        internal void SingletonImplementationTest()
        {
            SingletonImplementationTestInternal(
                () => DependencyContainer.UseVersion<SingletonImplementation, SingletonImplementationV2>(),
                () => DependencyContainer.UseVersion<SingletonImplementation, SingletonImplementationV3>());

            SingletonImplementationTestInternal(
                () => DependencyContainer.UseVersion<SingletonImplementation>(() => new SingletonImplementationV2()),
                () => DependencyContainer.UseVersion<SingletonImplementation>(() => new SingletonImplementationV3()));
        }

        internal void SingletonImplementationTestInternal(Func<IDisposable> applyFirst, Func<IDisposable> applySecond)
        {
            var original = new[]
                           {
                               typeof(SingletonImplementationDecorator),
                               typeof(SingletonImplementation),
                           };

            var first = new[]
                        {
                            typeof(SingletonImplementationDecorator),
                            typeof(SingletonImplementationV2),
                        };

            var second = new[]
                         {
                             typeof(SingletonImplementationDecorator),
                             typeof(SingletonImplementationV3),
                         };

            using (DependencyContainer.OpenScope())
            {
                VerifyNested(applyFirst,
                             applySecond,
                             () => Check(typeof(SingletonImplementation), original, original),
                             () => Check(typeof(SingletonImplementationV2), original, first),
                             () => Check(typeof(SingletonImplementationV3), original, second));
            }

            void Check(Type currentVersion,
                       ICollection<Type> originalStructure,
                       ICollection<Type> currentStructure)
            {
                var expectedVersionsTypes = new[]
                                         {
                                             typeof(SingletonImplementationV2),
                                             typeof(SingletonImplementationV3)
                                         };

                CheckSingleton(ResolveOriginal, ResolveVersions, ResolveVersioned, typeof(SingletonImplementation), currentVersion, expectedVersionsTypes);
                CheckDecorators(ResolveOriginal, ResolveVersions, ResolveVersioned, originalStructure, currentStructure, expectedVersionsTypes);
            }

            SingletonImplementation ResolveOriginal() => DependencyContainer.Resolve<SingletonImplementation>();
            IEnumerable<SingletonImplementation> ResolveVersions() => DependencyContainer.ResolveCollection<IVersionFor<SingletonImplementation>>().Select(z => z.Version);
            IVersioned<SingletonImplementation> ResolveVersioned() => DependencyContainer.Resolve<IVersioned<SingletonImplementation>>();
        }

        private static void CheckTransient<TTransient>(Func<TTransient> resolveOriginal,
                                                       Func<IEnumerable<TTransient>> resolveVersions,
                                                       Func<IVersioned<TTransient>> resolveVersioned,
                                                       Type originalVersion,
                                                       Type currentVersion,
                                                       Type[] expectedVersionsTypes)
            where TTransient : class
        {
            // original
            Assert.Equal(originalVersion, UnwrapDecorators(resolveOriginal()).GetType());
            Assert.NotSame(resolveOriginal(), resolveOriginal());
            Assert.NotSame(UnwrapDecorators(resolveOriginal()), UnwrapDecorators(resolveOriginal()));
            Assert.Equal(originalVersion, UnwrapDecorators(resolveVersioned().Original).GetType());
            Assert.NotSame(resolveVersioned().Original, resolveVersioned().Original);
            Assert.NotSame(UnwrapDecorators(resolveVersioned().Original), UnwrapDecorators(resolveVersioned().Original));

            // versions
            Assert.True(expectedVersionsTypes.SequenceEqual(Types(resolveVersions())));
            Assert.True(expectedVersionsTypes.SequenceEqual(Types(resolveVersioned().Versions)));

            // versioned
            Assert.NotSame(resolveVersioned(), resolveVersioned());
            Assert.Equal(currentVersion, UnwrapDecorators(resolveVersioned().Current).GetType());
            Assert.NotSame(resolveVersioned().Current, resolveVersioned().Current);
            Assert.NotSame(UnwrapDecorators(resolveVersioned().Current), UnwrapDecorators(resolveVersioned().Current));
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
            Assert.Equal(originalVersion, UnwrapDecorators(resolveOriginal()).GetType());
            Assert.Same(resolveOriginal(), resolveOriginal());
            Assert.Same(UnwrapDecorators(resolveOriginal()), UnwrapDecorators(resolveOriginal()));
            var outer = resolveOriginal();
            using (DependencyContainer.OpenScope())
            {
                Assert.NotSame(outer, resolveOriginal());
                Assert.NotSame(UnwrapDecorators(outer), UnwrapDecorators(resolveOriginal()));
            }

            Assert.Equal(originalVersion, UnwrapDecorators(resolveVersioned().Original).GetType());
            Assert.Same(resolveVersioned().Original, resolveVersioned().Original);
            Assert.Same(UnwrapDecorators(resolveVersioned().Original), UnwrapDecorators(resolveVersioned().Original));
            outer = resolveVersioned().Original;
            using (DependencyContainer.OpenScope())
            {
                Assert.NotSame(outer, resolveVersioned().Original);
                Assert.NotSame(UnwrapDecorators(outer), UnwrapDecorators(resolveVersioned().Original));
            }

            // versions
            Assert.True(expectedVersionsTypes.SequenceEqual(Types(resolveVersions())));
            Assert.True(expectedVersionsTypes.SequenceEqual(Types(resolveVersioned().Versions)));

            // versioned
            Assert.Same(resolveVersioned(), resolveVersioned());
            var outerVersioned = resolveVersioned();
            using (DependencyContainer.OpenScope())
            {
                Assert.NotSame(outerVersioned, resolveVersioned());
            }

            Assert.Equal(currentVersion, UnwrapDecorators(resolveVersioned().Current).GetType());
            Assert.Same(resolveVersioned().Current, resolveVersioned().Current);
            Assert.Same(UnwrapDecorators(resolveVersioned().Current), UnwrapDecorators(resolveVersioned().Current));
            outer = resolveVersioned().Current;
            using (DependencyContainer.OpenScope())
            {
                Assert.NotSame(outer, resolveVersioned().Current);
                Assert.NotSame(UnwrapDecorators(outer), UnwrapDecorators(resolveVersioned().Current));
            }
        }

        private static void CheckSingleton<TSingleton>(Func<TSingleton> resolveOriginal,
                                                       Func<IEnumerable<TSingleton>> resolveVersions,
                                                       Func<IVersioned<TSingleton>> resolveVersioned,
                                                       Type originalVersion,
                                                       Type currentVersion,
                                                       Type[] expectedVersionsTypes)
            where TSingleton : class
        {
            // original
            Assert.Equal(originalVersion, UnwrapDecorators(resolveOriginal()).GetType());
            Assert.Same(resolveOriginal(), resolveOriginal());
            Assert.Same(UnwrapDecorators(resolveOriginal()), UnwrapDecorators(resolveOriginal()));
            Assert.Equal(originalVersion, UnwrapDecorators(resolveVersioned().Original).GetType());
            Assert.Same(resolveVersioned().Original, resolveVersioned().Original);
            Assert.Same(UnwrapDecorators(resolveVersioned().Original), UnwrapDecorators(resolveVersioned().Original));

            // versions
            Assert.True(expectedVersionsTypes.SequenceEqual(Types(resolveVersions())));
            Assert.True(expectedVersionsTypes.SequenceEqual(Types(resolveVersioned().Versions)));

            // versioned
            Assert.Same(resolveVersioned(), resolveVersioned());
            Assert.Equal(currentVersion, UnwrapDecorators(resolveVersioned().Current).GetType());
            Assert.Same(resolveVersioned().Current, resolveVersioned().Current);
            Assert.Same(UnwrapDecorators(resolveVersioned().Current), UnwrapDecorators(resolveVersioned().Current));
        }

        private void CheckDecorators<TService>(Func<TService> resolveOriginal,
                                               Func<IEnumerable<TService>> resolveVersions,
                                               Func<IVersioned<TService>> resolveVersioned,
                                               ICollection<Type> originalStructure,
                                               ICollection<Type> currentStructure,
                                               ICollection<Type> resolvedVersions)
            where TService : class
        {
            Assert.True(originalStructure.SequenceEqual(resolveOriginal().ExtractDecorators()));
            Assert.True(originalStructure.SequenceEqual(resolveVersioned().Original.ExtractDecorators()));
            Assert.True(currentStructure.SequenceEqual(resolveVersioned().Current.ExtractDecorators().ShowTypes(nameof(currentStructure), Output.WriteLine)));
            Assert.True(resolvedVersions.SequenceEqual(Types(resolveVersions())));
            Assert.True(resolvedVersions.SequenceEqual(Types(resolveVersioned().Versions)));
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

        private static object UnwrapDecorators<TService>(TService service)
            where TService : class
        {
            while (service is IDecorator<TService> decorator)
            {
                service = decorator.Decoratee;
            }

            return service;
        }
    }
}