namespace SpaceEngineers.Core.Modules.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using AutoRegistration.Abstractions;
    using AutoWiring.Api.Abstractions;
    using AutoWiring.Api.Enumerations;
    using AutoWiringTest;
    using Basics.Test;
    using Core.Test.Api.ClassFixtures;
    using GenericEndpoint.Abstractions;
    using GenericEndpoint.Contract.Abstractions;
    using GenericHost;
    using Registrations;
    using VersionedContainer;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// DependencyContainer decorators test class
    /// </summary>
    public class DependencyContainerDecoratorsTest : BasicsTestBase, IClassFixture<ModulesTestFixture>
    {
        private readonly ModulesTestFixture _fixture;
        private readonly Assembly[] _excludedAssemblies;

        /// <summary> .ctor </summary>
        /// <param name="output">ITestOutputHelper</param>
        /// <param name="fixture">ModulesTestFixture</param>
        public DependencyContainerDecoratorsTest(ITestOutputHelper output, ModulesTestFixture fixture)
            : base(output)
        {
            _excludedAssemblies = new[]
            {
                typeof(IIntegrationMessage).Assembly, // GenericEndpoint.Contract
                typeof(IGenericEndpoint).Assembly, // GenericEndpoint
                typeof(GenericHost).Assembly // GenericHost
            };

            var registrations = new IManualRegistration[]
            {
                new VersionedOpenGenericRegistration()
            };

            DependencyContainer = fixture.GetDependencyContainer(typeof(DependencyContainerDecoratorsTest).Assembly, _excludedAssemblies, registrations);

            _fixture = fixture;
        }

        /// <summary>
        /// DependencyContainer
        /// </summary>
        protected IDependencyContainer DependencyContainer { get; }

        [Fact]
        internal void CheckDecoratorsOnVersionedServicesTest()
        {
            var registrations = new IManualRegistration[]
            {
                new VersionedOpenGenericRegistration(),
                new RegisterVersionedOpenGenerics()
            };

            var localContainer = _fixture.GetDependencyContainer(GetType().Assembly, _excludedAssemblies, registrations);

            // non-generic
            var nonGenericServiceExpected = new[]
                                            {
                                                typeof(TransientVersionedServiceDecorator),
                                                typeof(TransientVersionedServiceImpl)
                                            };
            var nonGenericServiceImplExpected = new[]
                                                {
                                                    typeof(TransientVersionedServiceImpl)
                                                };

            var nonGenericService = localContainer.Resolve<IVersioned<ITransientVersionedService>>().Original;
            var nonGenericServiceImpl = localContainer.Resolve<IVersioned<TransientVersionedServiceImpl>>().Original;

            Assert.True(nonGenericServiceExpected.SequenceEqual(nonGenericService.ExtractDecorators().ShowTypes("#1", Output.WriteLine)));
            Assert.True(nonGenericServiceImplExpected.SequenceEqual(nonGenericServiceImpl.ExtractDecorators().ShowTypes("#2", Output.WriteLine)));

            // open-generic
            var genericServiceExpected = new[]
                                         {
                                             typeof(OpenGenericDecorableServiceDecorator1<object>),
                                             typeof(OpenGenericDecorableServiceDecorator2<object>),
                                             typeof(OpenGenericDecorableServiceDecorator3<object>),
                                             typeof(OpenGenericDecorableServiceImpl<object>)
                                         };
            var genericServiceImplExpected = new[]
                                             {
                                                 typeof(OpenGenericDecorableServiceImpl<object>)
                                             };

            var genericService = localContainer.Resolve<IVersioned<IOpenGenericDecorableService<object>>>().Original;
            var genericServiceImpl = localContainer.Resolve<IVersioned<OpenGenericDecorableServiceImpl<object>>>().Original;

            Assert.True(genericServiceExpected.SequenceEqual(genericService.ExtractDecorators().ShowTypes("#3", Output.WriteLine)));
            Assert.True(genericServiceImplExpected.SequenceEqual(genericServiceImpl.ExtractDecorators().ShowTypes("#4", Output.WriteLine)));
        }

        [Fact]
        internal void DecoratorTest()
        {
            var service = DependencyContainer.Resolve<IDecorableService>();

            var types = new Dictionary<Type, Type>
            {
                [typeof(DecorableServiceDecorator1)] = typeof(DecorableServiceDecorator2),
                [typeof(DecorableServiceDecorator2)] = typeof(DecorableServiceDecorator3),
                [typeof(DecorableServiceDecorator3)] = typeof(DecorableServiceImpl)
            };

            void CheckRecursive(IDecorableService resolved, Type type)
            {
                Assert.True(resolved.GetType() == type);
                Output.WriteLine(type.Name);

                if (types.TryGetValue(type, out var nextDecorateeType))
                {
                    var decorator = (IDecorableServiceDecorator)resolved;
                    CheckRecursive(decorator.Decoratee, nextDecorateeType);
                }
            }

            CheckRecursive(service, typeof(DecorableServiceDecorator1));
        }

        [Fact]
        internal void OpenGenericDecoratorTest()
        {
            var service = DependencyContainer.Resolve<IOpenGenericDecorableService<object>>();

            var types = new Dictionary<Type, Type>
            {
                [typeof(OpenGenericDecorableServiceDecorator1<object>)] = typeof(OpenGenericDecorableServiceDecorator2<object>),
                [typeof(OpenGenericDecorableServiceDecorator2<object>)] = typeof(OpenGenericDecorableServiceDecorator3<object>),
                [typeof(OpenGenericDecorableServiceDecorator3<object>)] = typeof(OpenGenericDecorableServiceImpl<object>)
            };

            void CheckRecursive(IOpenGenericDecorableService<object> resolved, Type type)
            {
                Assert.True(resolved.GetType() == type);
                Output.WriteLine(type.Name);

                if (types.TryGetValue(type, out var nextDecorateeType))
                {
                    var decorator = (IOpenGenericDecorableServiceDecorator<object>)resolved;
                    CheckRecursive(decorator.Decoratee, nextDecorateeType);
                }
            }

            CheckRecursive(service, typeof(OpenGenericDecorableServiceDecorator1<object>));
        }

        [Fact]
        internal void ConditionalDecoratorTest()
        {
            var service = DependencyContainer.Resolve<IConditionalDecorableService>();

            var types = new Dictionary<Type, Type>
            {
                [typeof(ConditionalDecorableServiceDecorator1)] = typeof(ConditionalDecorableServiceDecorator3),
                [typeof(ConditionalDecorableServiceDecorator3)] = typeof(ConditionalDecorableServiceImpl)
            };

            void CheckRecursive(IConditionalDecorableService resolved, Type type)
            {
                Output.WriteLine($"{resolved.GetType()} == {type.Name}");
                Assert.True(resolved.GetType() == type);

                if (types.TryGetValue(type, out var nextDecorateeType))
                {
                    var decorator = (IDecorator<IConditionalDecorableService>)resolved;
                    CheckRecursive(decorator.Decoratee, nextDecorateeType);
                }
            }

            CheckRecursive(service, typeof(ConditionalDecorableServiceDecorator1));
        }

        [Fact]
        internal void CollectionResolvableConditionDecorableTest()
        {
            var services = DependencyContainer.ResolveCollection<ICollectionResolvableConditionDecorableService>()
                                              .ToArray();

            var types = new[]
                        {
                            new Dictionary<Type, Type>
                            {
                                [typeof(CollectionResolvableConditionDecorableServiceDecorator3)] = typeof(CollectionResolvableConditionDecorableServiceImpl3)
                            },
                            new Dictionary<Type, Type>
                            {
                                [typeof(CollectionResolvableConditionDecorableServiceDecorator3)] = typeof(CollectionResolvableConditionDecorableServiceDecorator2),
                                [typeof(CollectionResolvableConditionDecorableServiceDecorator2)] = typeof(CollectionResolvableConditionDecorableServiceImpl2)
                            },
                            new Dictionary<Type, Type>
                            {
                                [typeof(CollectionResolvableConditionDecorableServiceDecorator3)] = typeof(CollectionResolvableConditionDecorableServiceDecorator1),
                                [typeof(CollectionResolvableConditionDecorableServiceDecorator1)] = typeof(CollectionResolvableConditionDecorableServiceImpl1)
                            }
                        };

            void CheckRecursive(ICollectionResolvableConditionDecorableService resolved, int i, Type type)
            {
                Output.WriteLine($"{resolved.GetType()} == {type.Name}");
                Assert.True(resolved.GetType() == type);

                if (types[i].TryGetValue(type, out var nextDecorateeType))
                {
                    var decorator = (ICollectionDecorator<ICollectionResolvableConditionDecorableService>)resolved;

                    CheckRecursive(decorator.Decoratee, i, nextDecorateeType);
                }
            }

            CheckRecursive(services[0], 0, typeof(CollectionResolvableConditionDecorableServiceDecorator3));
            Output.WriteLine(string.Empty);
            CheckRecursive(services[1], 1, typeof(CollectionResolvableConditionDecorableServiceDecorator3));
            Output.WriteLine(string.Empty);
            CheckRecursive(services[2], 2, typeof(CollectionResolvableConditionDecorableServiceDecorator3));
        }

        private class RegisterVersionedOpenGenerics : IManualRegistration
        {
            public void Register(IManualRegistrationsContainer container)
            {
                container.RegisterVersioned<IOpenGenericDecorableService<object>>(EnLifestyle.Transient);
                container.RegisterVersioned<OpenGenericDecorableServiceImpl<object>>(EnLifestyle.Transient);
            }
        }
    }
}