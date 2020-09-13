namespace SpaceEngineers.Core.Modules.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoRegistration.Implementations;
    using AutoWiringApi.Abstractions;
    using AutoWiringApi.Enumerations;
    using AutoWiringTest;
    using Basics;
    using VersionedContainer;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// DependencyContainer decorators test class
    /// </summary>
    public class DependencyContainerDecoratorsTest : ModulesTestBase
    {
        /// <summary> .ctor </summary>
        /// <param name="output">ITestOutputHelper</param>
        public DependencyContainerDecoratorsTest(ITestOutputHelper output)
            : base(output)
        {
        }

        [Fact]
        internal void CheckDecoratorsOnVersionedServicesTest()
        {
            var localContainer = SetupDependencyContainer(container =>
                                                          {
                                                              container.RegisterVersion<IOpenGenericDecorableService<object>>(EnLifestyle.Transient);
                                                              container.RegisterVersion<OpenGenericDecorableServiceImpl<object>>(EnLifestyle.Transient);
                                                          });

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
                                             typeof(OpenGenericDecorableServiceImpl<object>),
                                         };
            var genericServiceImplExpected = new[]
                                             {
                                                 typeof(OpenGenericDecorableServiceImpl<object>),
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
                [typeof(DecorableServiceDecorator3)] = typeof(DecorableServiceImpl),
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
                [typeof(OpenGenericDecorableServiceDecorator3<object>)] = typeof(OpenGenericDecorableServiceImpl<object>),
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
                [typeof(ConditionalDecorableServiceDecorator3)] = typeof(ConditionalDecorableServiceImpl),
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
                                [typeof(CollectionResolvableConditionDecorableServiceDecorator2)] = typeof(CollectionResolvableConditionDecorableServiceImpl2),
                            },
                            new Dictionary<Type, Type>
                            {
                                [typeof(CollectionResolvableConditionDecorableServiceDecorator3)] = typeof(CollectionResolvableConditionDecorableServiceDecorator1),
                                [typeof(CollectionResolvableConditionDecorableServiceDecorator1)] = typeof(CollectionResolvableConditionDecorableServiceImpl1),
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
    }
}