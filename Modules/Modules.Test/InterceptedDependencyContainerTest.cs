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
        internal void DecoratorAsDependencyTest()
        {
            VerifyNested(() => DependencyContainer.Resolve<IServiceWithOverrideAsDependency>().ServiceForInterception);
        }

        [Fact]
        internal void SeveralDependenciesTest()
        {
            VerifyNested(() => DependencyContainer.Resolve<IServiceWithSeveralDependenciesForOverride>().ServiceForInterception);
            VerifyNested(() => DependencyContainer.Resolve<IServiceWithSeveralDependenciesForOverride>().ServiceWithOverrideAsDependency.ServiceForInterception);
        }

        [Fact]
        internal void ApplyOnRegisteredDecoratorsTest()
        {
            var resolves = new Func<IServiceForInterception>[]
                           {
                               () => DependencyContainer.Resolve<IServiceForInterception>(),
                               () => DependencyContainer.Resolve<IServiceWithSeveralDependenciesForOverride>().ServiceForInterception,
                               () => DependencyContainer.Resolve<IServiceWithSeveralDependenciesForOverride>().ServiceWithOverrideAsDependency.ServiceForInterception,
                               () => DependencyContainer.Resolve<IServiceWithOverrideAsDependency>().ServiceForInterception,
                           };

            foreach (var resolve in resolves)
            {
                VerifyNested(
                    () => DependencyContainer.ApplyDecorator<IServiceForInterception, RegisteredDecorator>(),
                    () => DependencyContainer.ApplyDecorator<IServiceForInterception, RegisteredDecoratorWithExtraDependency>(),
                    () => VerifyError(resolve),
                    () => VerifyError(resolve),
                    () => VerifyError(resolve));
            }
        }

        [Fact]
        internal void DecoratorWithExtraDependencyTest()
        {
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

            var resolves = new Func<IServiceForInterception>[]
                {
                    () => DependencyContainer.Resolve<IServiceForInterception>(),
                    () => DependencyContainer.Resolve<IServiceWithSeveralDependenciesForOverride>().ServiceForInterception,
                    () => DependencyContainer.Resolve<IServiceWithSeveralDependenciesForOverride>().ServiceWithOverrideAsDependency.ServiceForInterception,
                    () => DependencyContainer.Resolve<IServiceWithOverrideAsDependency>().ServiceForInterception,
                };

            foreach (var resolve in resolves)
            {
                VerifyNested(
                    () => DependencyContainer.ApplyDecorator<IServiceForInterception, UnregisteredDecoratorWithExtraDependency>(),
                    () => DependencyContainer.ApplyDecorator<IServiceForInterception, UnregisteredDecorator>(),
                    () => Compare<IServiceForInterception, IServiceForInterceptionDecorator>("outer", outer, resolve()),
                    () => Compare<IServiceForInterception, IServiceForInterceptionDecorator>("first", first, resolve()),
                    () => Compare<IServiceForInterception, IServiceForInterceptionDecorator>("second", second, resolve()));
            }
        }

        [Fact]
        internal void DecorateExtraDependenciesTest()
        {
            var outer = new[]
                        {
                            typeof(RegisteredDecorator),
                            typeof(RegisteredDecoratorWithExtraDependency),
                            typeof(ServiceForInterceptionImpl)
                        };

            var resolves = new Func<IServiceForInterception>[]
                {
                    () => DependencyContainer.Resolve<IServiceForInterception>(),
                    () => DependencyContainer.Resolve<IServiceWithSeveralDependenciesForOverride>().ServiceForInterception,
                    () => DependencyContainer.Resolve<IServiceWithSeveralDependenciesForOverride>().ServiceWithOverrideAsDependency.ServiceForInterception,
                    () => DependencyContainer.Resolve<IServiceWithOverrideAsDependency>().ServiceForInterception,
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
                throw new NotImplementedException();
            }

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

            void Check(string tag,
                       Func<IServiceForInterception> resolve,
                       Type[] mainGraphExpected,
                       Type[] extraExpected,
                       Type[] implExtraExpected)
            {
                // 1. IServiceForInterception
                var resolved = resolve();
                Compare<IServiceForInterception, IServiceForInterceptionDecorator>(tag, mainGraphExpected, resolved);
                var decoratorWithExtraDependency = (RegisteredDecoratorWithExtraDependency)((RegisteredDecorator)resolved).Decoratee;

                // 2. _extra
                var extra = decoratorWithExtraDependency.GetFieldValue<IExtraDependency>("_extra");
                Compare<IExtraDependency, IExtraDependencyDecorator>(tag + "_" + nameof(extra), extraExpected, extra);

                // 3. _implExtra
                var implExtra = decoratorWithExtraDependency.GetFieldValue<ImplementationExtra>("_implExtra");
                Compare<ImplementationExtra, ImplementationExtraDecorator>(tag + "_" + nameof(implExtra), implExtraExpected, implExtra);
            }

            foreach (var resolve in resolves)
            {
                VerifyNested(
                    ApplyFirst,
                    ApplySecond,
                    () => Check("outer", resolve, outer, outerExtraExpected, outerImplExtraExpected),
                    () => Check("first", resolve, outer, firstExtraExpected, firstImplExtraExpected),
                    () => Check("second", resolve, outer, firstExtraExpected, firstImplExtraExpected));
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

        private void VerifyError(Func<IServiceForInterception> resolve)
        {
            resolve.Try()
                   .Catch<ActivationException>(ex =>
                                               {
                                                   if (!(ex.InnerException?.RealException() is InvalidOperationException))
                                                   {
                                                       throw ex;
                                                   }
                                               })
                   .Invoke();
        }

        private static IEnumerable<Type> ExtractDecorators<TService, TDecorator>(TService obj)
            where TService : class
            where TDecorator : IDecorator<TService>
        {
            yield return obj.GetType();

            if (obj is TDecorator decorator)
            {
                foreach (var type in ExtractDecorators<TService, TDecorator>(decorator.Decoratee))
                {
                    yield return type;
                }
            }
        }

        private void Compare<TService, TDecorator>(string label, IEnumerable<Type> expected, TService service)
            where TService : class
            where TDecorator : IDecorator<TService>
        {
            var actual = ExtractDecorators<TService, TDecorator>(service).ToList();
            Output.WriteLine(label + Environment.NewLine + string.Join(Environment.NewLine, actual.Select(type => type.FullName)));
            Assert.True(expected.SequenceEqual(actual));
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