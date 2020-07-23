namespace SpaceEngineers.Core.Modules.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Disposables;
    using AutoWiringApi.Abstractions;
    using Basics;
    using InterceptedContainerTest;
    using SimpleInjector;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// IInterceptedContainer class test
    /// </summary>
    public class InterceptedDependencyContainerTest : ModulesTestBase
    {
        /// <summary> .ctor </summary>
        /// <param name="output">ITestOutputHelper</param>
        public InterceptedDependencyContainerTest(ITestOutputHelper output)
            : base(output)
        {
        }

        [Fact]
        internal void SimpleResolveTest()
        {
            VerifyNested(() => DependencyContainer.Resolve<IServiceForInterception>());
        }

        [Fact]
        internal void DecoratedDependencyTest()
        {
            VerifyNested(() => DependencyContainer.Resolve<IServiceWithDecoratedDependency>().ServiceForInterception);
        }

        [Fact]
        internal void SeveralDependenciesTest()
        {
            VerifyNested(() => DependencyContainer.Resolve<IServiceWithSeveralDependencies>().ServiceForInterception);
            VerifyNested(() => DependencyContainer.Resolve<IServiceWithSeveralDependencies>().ServiceWithDecoratedDependency.ServiceForInterception);
        }

        [Fact]
        internal void ApplyOnRegisteredDecoratorsTest()
        {
            var resolves = new Func<IServiceForInterception>[]
                           {
                               () => DependencyContainer.Resolve<IServiceForInterception>(),
                               () => DependencyContainer.Resolve<IServiceWithSeveralDependencies>().ServiceForInterception,
                               () => DependencyContainer.Resolve<IServiceWithSeveralDependencies>().ServiceWithDecoratedDependency.ServiceForInterception,
                               () => DependencyContainer.Resolve<IServiceWithDecoratedDependency>().ServiceForInterception,
                           };

            foreach (var resolve in resolves)
            {
                VerifyNested(
                    () => DependencyContainer.ApplyDecorator<IServiceForInterception, RegisteredDecorator>(),
                    () => DependencyContainer.ApplyDecorator<IServiceForInterception, RegisteredDecoratorWithExtraDependency>(),
                    () => resolve(),
                    () => VerifyError(resolve, $"Decorator {typeof(RegisteredDecorator)} already registered in container"),
                    () => VerifyError(resolve, $"Decorator {typeof(RegisteredDecoratorWithExtraDependency)} already registered in container"));
            }

            using (DependencyContainer.ApplyDecorator<ImplementationExtra, RegisteredImplementationExtraDecorator>())
            {
                VerifyError(() => DependencyContainer.Resolve<IServiceForInterception>(), $"Decorator {typeof(RegisteredImplementationExtraDecorator)} already registered in container");
            }
        }

        [Fact]
        internal void DecorateUnregisteredExtraDependenciesTest()
        {
            var resolves = new Func<IServiceForInterception>[]
                           {
                               () => DependencyContainer.Resolve<IServiceForInterception>(),
                               () => DependencyContainer.Resolve<IServiceWithSeveralDependencies>().ServiceForInterception,
                               () => DependencyContainer.Resolve<IServiceWithSeveralDependencies>().ServiceWithDecoratedDependency.ServiceForInterception,
                               () => DependencyContainer.Resolve<IServiceWithDecoratedDependency>().ServiceForInterception,
                           };

            IDisposable ApplyFirst()
            {
                return new CompositeDisposable
                       {
                           DependencyContainer.ApplyDecorator<IServiceForInterception, UnregisteredDecoratorWithExtraDependency>(),
                           DependencyContainer.ApplyDecorator<IExtraDependency, UnregisteredExtraDependencyDecorator>(),
                           DependencyContainer.ApplyDecorator<ImplementationExtra, UnregisteredImplementationExtraDecorator>()
                       };
            }

            IDisposable ApplySecond()
            {
                return new CompositeDisposable
                    {
                        DependencyContainer.ApplyDecorator<IServiceForInterception, UnregisteredDecorator>(),
                        DependencyContainer.ApplyDecorator<IExtraDependency, AnotherUnregisteredExtraDependencyDecorator>(),
                        DependencyContainer.ApplyDecorator<ImplementationExtra, AnotherUnregisteredImplementationExtraDecorator>()
                    };
            }

            var outer = new[]
                        {
                            typeof(RegisteredDecorator),
                            typeof(RegisteredDecoratorWithExtraDependency),
                            typeof(ServiceForInterceptionImpl)
                        };

            var first = new[]
                        {
                            typeof(UnregisteredDecoratorWithExtraDependency),
                            typeof(RegisteredDecorator),
                            typeof(RegisteredDecoratorWithExtraDependency),
                            typeof(ServiceForInterceptionImpl)
                        };

            var second = new[]
                         {
                             typeof(UnregisteredDecorator),
                             typeof(UnregisteredDecoratorWithExtraDependency),
                             typeof(RegisteredDecorator),
                             typeof(RegisteredDecoratorWithExtraDependency),
                             typeof(ServiceForInterceptionImpl)
                         };

            var outerExtraExpected = new[]
                                     {
                                         typeof(RegisteredExtraDependencyDecorator),
                                         typeof(ExtraDependencyImpl),
                                     };

            var outerImplExtraExpected = new[]
                                         {
                                             typeof(RegisteredImplementationExtraDecorator),
                                             typeof(ImplementationExtra),
                                         };

            var firstExtraExpected = new[]
                                     {
                                         typeof(UnregisteredExtraDependencyDecorator),
                                         typeof(RegisteredExtraDependencyDecorator),
                                         typeof(ExtraDependencyImpl),
                                     };

            var firstImplExtraExpected = new[]
                                         {
                                             typeof(UnregisteredImplementationExtraDecorator),
                                             typeof(RegisteredImplementationExtraDecorator),
                                             typeof(ImplementationExtra),
                                         };

            var secondExtraExpected = new[]
                                      {
                                          typeof(AnotherUnregisteredExtraDependencyDecorator),
                                          typeof(UnregisteredExtraDependencyDecorator),
                                          typeof(RegisteredExtraDependencyDecorator),
                                          typeof(ExtraDependencyImpl),
                                      };

            var secondImplExtraExpected = new[]
                                          {
                                              typeof(AnotherUnregisteredImplementationExtraDecorator),
                                              typeof(UnregisteredImplementationExtraDecorator),
                                              typeof(RegisteredImplementationExtraDecorator),
                                              typeof(ImplementationExtra),
                                          };

            void Check(string tag,
                       Func<IServiceForInterception> resolve,
                       Type[] mainGraphExpected,
                       Type[] extraExpected,
                       Type[] implExtraExpected)
            {
                var resolved = resolve();
                Compare<IServiceForInterception, IServiceForInterceptionDecorator>(tag, mainGraphExpected, resolved);
                CheckExtra<IServiceForInterception, IServiceForInterceptionDecorator>(tag, resolved, extraExpected, implExtraExpected);
            }

            foreach (var resolve in resolves)
            {
                VerifyNested(
                    ApplyFirst,
                    ApplySecond,
                    () => Check("outer", resolve, outer, outerExtraExpected, outerImplExtraExpected),
                    () => Check("first", resolve, first, firstExtraExpected, firstImplExtraExpected),
                    () => Check("second", resolve, second, secondExtraExpected, secondImplExtraExpected));
            }
        }

        [Fact]
        internal void DecorateRegisteredExtraDependenciesTest()
        {
            var resolves = new Func<IServiceForInterception>[]
                           {
                               () => DependencyContainer.Resolve<IServiceForInterception>(),
                               () => DependencyContainer.Resolve<IServiceWithSeveralDependencies>().ServiceForInterception,
                               () => DependencyContainer.Resolve<IServiceWithSeveralDependencies>().ServiceWithDecoratedDependency.ServiceForInterception,
                               () => DependencyContainer.Resolve<IServiceWithDecoratedDependency>().ServiceForInterception,
                           };

            IDisposable ApplyFirst()
            {
                return new CompositeDisposable
                       {
                           DependencyContainer.ApplyDecorator<IExtraDependency, UnregisteredExtraDependencyDecorator>(),
                           DependencyContainer.ApplyDecorator<ImplementationExtra, UnregisteredImplementationExtraDecorator>()
                       };
            }

            IDisposable ApplySecond()
            {
                return new CompositeDisposable
                       {
                           DependencyContainer.ApplyDecorator<IExtraDependency, AnotherUnregisteredExtraDependencyDecorator>(),
                           DependencyContainer.ApplyDecorator<ImplementationExtra, AnotherUnregisteredImplementationExtraDecorator>()
                       };
            }

            var outer = new[]
                        {
                            typeof(RegisteredDecorator),
                            typeof(RegisteredDecoratorWithExtraDependency),
                            typeof(ServiceForInterceptionImpl)
                        };

            var outerExtraExpected = new[]
                                     {
                                         typeof(RegisteredExtraDependencyDecorator),
                                         typeof(ExtraDependencyImpl),
                                     };

            var outerImplExtraExpected = new[]
                                         {
                                             typeof(RegisteredImplementationExtraDecorator),
                                             typeof(ImplementationExtra),
                                         };

            var firstExtraExpected = new[]
                                     {
                                         typeof(UnregisteredExtraDependencyDecorator),
                                         typeof(RegisteredExtraDependencyDecorator),
                                         typeof(ExtraDependencyImpl),
                                     };

            var firstImplExtraExpected = new[]
                                         {
                                             typeof(UnregisteredImplementationExtraDecorator),
                                             typeof(RegisteredImplementationExtraDecorator),
                                             typeof(ImplementationExtra),
                                         };

            var secondExtraExpected = new[]
                                      {
                                          typeof(AnotherUnregisteredExtraDependencyDecorator),
                                          typeof(UnregisteredExtraDependencyDecorator),
                                          typeof(RegisteredExtraDependencyDecorator),
                                          typeof(ExtraDependencyImpl),
                                      };

            var secondImplExtraExpected = new[]
                                          {
                                              typeof(AnotherUnregisteredImplementationExtraDecorator),
                                              typeof(UnregisteredImplementationExtraDecorator),
                                              typeof(RegisteredImplementationExtraDecorator),
                                              typeof(ImplementationExtra),
                                          };

            void Check(string tag,
                       Func<IServiceForInterception> resolve,
                       Type[] mainGraphExpected,
                       Type[] extraExpected,
                       Type[] implExtraExpected)
            {
                var resolved = resolve();
                Compare<IServiceForInterception, IServiceForInterceptionDecorator>(tag, mainGraphExpected, resolved);
                CheckExtra<IServiceForInterception, IServiceForInterceptionDecorator>(tag, resolved, extraExpected, implExtraExpected);
            }

            foreach (var resolve in resolves)
            {
                VerifyNested(
                    ApplyFirst,
                    ApplySecond,
                    () => Check("outer", resolve, outer, outerExtraExpected, outerImplExtraExpected),
                    () => Check("first", resolve, outer, firstExtraExpected, firstImplExtraExpected),
                    () => Check("second", resolve, outer, secondExtraExpected, secondImplExtraExpected));
            }
        }

        [Fact]
        internal void ApplyOrderTest()
        {
            var resolves = new Func<IServiceForInterception>[]
                           {
                               () => DependencyContainer.Resolve<IServiceForInterception>(),
                               () => DependencyContainer.Resolve<IServiceWithSeveralDependencies>().ServiceForInterception,
                               () => DependencyContainer.Resolve<IServiceWithSeveralDependencies>().ServiceWithDecoratedDependency.ServiceForInterception,
                               () => DependencyContainer.Resolve<IServiceWithDecoratedDependency>().ServiceForInterception,
                           };

            IDisposable A() => DependencyContainer.ApplyDecorator<IServiceForInterception, UnregisteredDecoratorWithExtraDependency>();
            IDisposable B() => DependencyContainer.ApplyDecorator<IExtraDependency, UnregisteredExtraDependencyDecorator>();
            IDisposable C() => DependencyContainer.ApplyDecorator<ImplementationExtra, UnregisteredImplementationExtraDecorator>();

            var applications = new Func<IDisposable>[]
                               {
                                   () => new CompositeDisposable { A(), B(), C() },
                                   () => new CompositeDisposable { A(), C(), B() },
                                   () => new CompositeDisposable { B(), A(), C() },
                                   () => new CompositeDisposable { B(), C(), A() },
                                   () => new CompositeDisposable { C(), A(), B() },
                                   () => new CompositeDisposable { C(), B(), A() },
                               };

            foreach (var app in applications)
            {
                using (app())
                {
                    foreach (var resolve in resolves)
                    {
                        resolve();
                    }
                }
            }
        }

        [Fact]
        internal void CyclicDependencyTest()
        {
            using (DependencyContainer.ApplyDecorator<IServiceForInterception, UnregisteredDecoratorCyclicReferenceProxy>())
            {
                VerifyError(() => DependencyContainer.Resolve<IServiceForInterception>(), string.Empty);
            }
        }

        [Fact]
        internal void OpenGenericTest()
        {
            throw new NotImplementedException();
        }

        [Fact]
        internal void ResolveCollectionTest()
        {
            throw new NotImplementedException();
        }

        [Fact]
        internal void LifestyleTest()
        {
            throw new NotImplementedException();
        }

        private void VerifyError(Func<IServiceForInterception> resolve, string msg)
        {
            Assert.Throws<ActivationException>(resolve);

            resolve.Try()
                   .Catch<ActivationException>(ex =>
                                               {
                                                   if (!(ex.InnerException?.RealException() is InvalidOperationException invalidOperationException))
                                                   {
                                                       throw ex;
                                                   }

                                                   Assert.Equal(msg, invalidOperationException.Message);
                                               })
                   .Invoke();
        }

        private static IEnumerable<TService> ExtractDecorators<TService, TDecorator>(TService obj)
            where TService : class
            where TDecorator : IDecorator<TService>
        {
            yield return obj;

            if (obj is TDecorator decorator)
            {
                foreach (var type in ExtractDecorators<TService, TDecorator>(decorator.Decoratee))
                {
                    yield return type;
                }
            }
        }

        private void Compare<TService, TDecorator>(string label, IEnumerable<Type> expected, TService resolved)
            where TService : class
            where TDecorator : IDecorator<TService>
        {
            var actual = ExtractDecorators<TService, TDecorator>(resolved).Select(d => d.GetType()).ToList();
            Output.WriteLine(label + Environment.NewLine + string.Join(Environment.NewLine, actual.Select(type => "\t" + type.FullName)));
            Assert.True(expected.SequenceEqual(actual));
        }

        private void CheckExtra<TService, TDecorator>(string tag, TService resolved, Type[] extraExpected, Type[] implExtraExpected)
            where TService : class
            where TDecorator : IDecorator<TService>
        {
            foreach (var decorator in ExtractDecorators<TService, TDecorator>(resolved))
            {
                if (decorator is IWithExtra withExtra)
                {
                    CheckExtra(withExtra);
                    CheckImplExtra(withExtra);
                }
            }

            void CheckExtra(IWithExtra withExtra)
            {
                var extra = withExtra.Extra;
                Compare<IExtraDependency, IExtraDependencyDecorator>(tag + "_" + nameof(extra) + " " + withExtra.GetType().FullName, extraExpected, extra);
            }

            void CheckImplExtra(IWithExtra withExtra)
            {
                var implExtra = withExtra.ImplExtra;
                Compare<ImplementationExtra, ImplementationExtraDecorator>(tag + "_" + nameof(implExtra) + " " + withExtra.GetType().FullName, implExtraExpected, implExtra);
            }
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

        private void VerifyNested(Func<IServiceForInterception> resolve)
        {
            var outer = new[]
                        {
                            typeof(RegisteredDecorator),
                            typeof(RegisteredDecoratorWithExtraDependency),
                            typeof(ServiceForInterceptionImpl)
                        };

            var first = new[]
                        {
                            typeof(UnregisteredDecorator),
                            typeof(RegisteredDecorator),
                            typeof(RegisteredDecoratorWithExtraDependency),
                            typeof(ServiceForInterceptionImpl)
                        };

            var second = new[]
                         {
                             typeof(AnotherUnregisteredDecoratorForInterception),
                             typeof(UnregisteredDecorator),
                             typeof(RegisteredDecorator),
                             typeof(RegisteredDecoratorWithExtraDependency),
                             typeof(ServiceForInterceptionImpl)
                         };

            VerifyNested(
                () => DependencyContainer.ApplyDecorator<IServiceForInterception, UnregisteredDecorator>(),
                () => DependencyContainer.ApplyDecorator<IServiceForInterception, AnotherUnregisteredDecoratorForInterception>(),
                () => Compare<IServiceForInterception, IServiceForInterceptionDecorator>("outer", outer, resolve()),
                () => Compare<IServiceForInterception, IServiceForInterceptionDecorator>("first", first, resolve()),
                () => Compare<IServiceForInterception, IServiceForInterceptionDecorator>("second", second, resolve()));
        }
    }
}