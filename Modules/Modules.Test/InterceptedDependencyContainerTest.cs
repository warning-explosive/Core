namespace SpaceEngineers.Core.Modules.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
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
        internal void SimpleResolveNestedScopeTest()
        {
            VerifyNested(() => DependencyContainer.Resolve<IServiceForInterception>());
        }

        [Fact]
        internal void DecoratorAsDependencyNestedScopeTest()
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
            var expected = new[]
                           {
                               typeof(RegisteredDecoratorForInterception),
                               typeof(ServiceForInterceptionImpl)
                           };

            IServiceForInterception Resolve() => DependencyContainer.Resolve<IServiceForInterception>();
            IServiceForInterception ResolveAsDependency1() => DependencyContainer.Resolve<IServiceWithSeveralDependenciesForOverride>().ServiceForInterception;
            IServiceForInterception ResolveAsDependency2() => DependencyContainer.Resolve<IServiceWithSeveralDependenciesForOverride>().ServiceWithOverrideAsDependency.ServiceForInterception;

            Verify<IServiceForInterception, IServiceForInterceptionDecorator>("#1", expected, Resolve());
            Verify<IServiceForInterception, IServiceForInterceptionDecorator>("#2", expected, ResolveAsDependency1());
            Verify<IServiceForInterception, IServiceForInterceptionDecorator>("#3", expected, ResolveAsDependency2());

            using (DependencyContainer.ApplyDecorator<IServiceForInterception, RegisteredDecoratorForInterception>())
            {
                VerifyError(Resolve);
                VerifyError(ResolveAsDependency1);
                VerifyError(ResolveAsDependency2);
            }
        }

        [Fact]
        internal void DecoratedImplementationAsDependencyTest()
        {
            throw new NotImplementedException();
        }

        [Fact]
        internal void RegisteredDecoratorWithExtraDependencyTest()
        {
            throw new NotImplementedException();
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

        private static IEnumerable<Type> Extract<TService, TDecorator>(TService obj)
            where TService : class
            where TDecorator : IDecorator<TService>
        {
            yield return obj.GetType();

            if (obj is TDecorator decorator)
            {
                foreach (var type in Extract<TService, TDecorator>(decorator.Decoratee))
                {
                    yield return type;
                }
            }
        }

        private void Verify<TService, TDecorator>(string label, IEnumerable<Type> expected, TService service)
            where TService : class
            where TDecorator : IDecorator<TService>
        {
            var actual = Extract<TService, TDecorator>(service).ToList();
            Output.WriteLine(label + Environment.NewLine + string.Join(Environment.NewLine, actual.Select(type => type.FullName)));
            Assert.True(expected.SequenceEqual(actual));
        }

        private void VerifyNested(Func<IServiceForInterception> resolve)
        {
            var expected = new[]
                           {
                               typeof(RegisteredDecoratorForInterception),
                               typeof(ServiceForInterceptionImpl)
                           };

            Verify<IServiceForInterception, IServiceForInterceptionDecorator>("#1", expected, resolve());

            using (DependencyContainer.ApplyDecorator<IServiceForInterception, UnregisteredDecoratorForInterception>())
            {
                var expectedOuterScoped = new[]
                                          {
                                              typeof(UnregisteredDecoratorForInterception),
                                              typeof(RegisteredDecoratorForInterception),
                                              typeof(ServiceForInterceptionImpl)
                                          };

                Verify<IServiceForInterception, IServiceForInterceptionDecorator>("#2", expectedOuterScoped, resolve());

                using (DependencyContainer.ApplyDecorator<IServiceForInterception, AnotherUnregisteredDecoratorForInterception>())
                {
                    var expectedInnerScoped = new[]
                                              {
                                                  typeof(AnotherUnregisteredDecoratorForInterception),
                                                  typeof(UnregisteredDecoratorForInterception),
                                                  typeof(RegisteredDecoratorForInterception),
                                                  typeof(ServiceForInterceptionImpl)
                                              };

                    Verify<IServiceForInterception, IServiceForInterceptionDecorator>("#3", expectedInnerScoped, resolve());
                }

                Verify<IServiceForInterception, IServiceForInterceptionDecorator>("#4", expectedOuterScoped, resolve());
            }

            Verify<IServiceForInterception, IServiceForInterceptionDecorator>("#5", expected, resolve());
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
    }
}