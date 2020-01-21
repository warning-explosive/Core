namespace SpaceEngineers.Core.CompositionRoot.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Basics;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// DependencyContainer class tests
    /// </summary>
    public class DependencyContainerTest : CompositionRootTestBase
    {
        /// <summary> .ctor </summary>
        /// <param name="output">ITestOutputHelper</param>
        public DependencyContainerTest(ITestOutputHelper output)
            : base(output) { }

        [Fact]
        internal void SingletonTest()
        {
            Assert.Equal(DependencyContainer.Resolve<ISingletonTestService>(),
                         DependencyContainer.Resolve<ISingletonTestService>());
        }

        [Fact]
        internal void TypedUntypedSingletonTest()
        {
            Assert.Equal(DependencyContainer.Resolve<ISingletonTestService>(),
                         DependencyContainer.Resolve(typeof(ISingletonTestService)));
        }

        [Fact]
        internal void OpenGenericTest()
        {
            Assert.NotNull(DependencyContainer.Resolve<IOpenGenericTestService<string>>());
        }

        [Fact]
        internal void AutoWiringSingletonTest()
        {
            Assert.Equal(DependencyContainer.Resolve<IWiredTestService>().IndependentTestService,
                         DependencyContainer.Resolve<IIndependentTestService>());
        }

        [Fact]
        internal void OrderedCollectionResolvableTest()
        {
            var resolvedTypes = DependencyContainer.ResolveCollection<ICollectionResolvableTestService>()
                                                   .Select(z => z.GetType());

            var types = new[]
            {
                typeof(CollectionResolvableTestServiceImpl3),
                typeof(CollectionResolvableTestServiceImpl2),
                typeof(CollectionResolvableTestServiceImpl1),
            };

            Assert.True(resolvedTypes.SequenceEqual(types));
        }

        [Fact]
        internal void UntypedOrderedCollectionResolvableTest()
        {
            var resolvedTypes = DependencyContainer.ResolveCollection(typeof(ICollectionResolvableTestService))
                                                   .Select(z => z.GetType());

            var types = new[]
            {
                typeof(CollectionResolvableTestServiceImpl3),
                typeof(CollectionResolvableTestServiceImpl2),
                typeof(CollectionResolvableTestServiceImpl1),
            };

            Assert.True(resolvedTypes.SequenceEqual(types));
        }

        [Fact]
        internal void SingletonOpenGenericCollectionResolvableTest()
        {
            var resolvedTypes = DependencyContainer.ResolveCollection<ISingletonGenericCollectionResolvableTestService<object>>()
                                                   .Select(z => z.GetType());

            var types = new[]
                        {
                            typeof(SingletonGenericCollectionResolvableTestServiceImpl3<object>),
                            typeof(SingletonGenericCollectionResolvableTestServiceImpl2<object>),
                            typeof(SingletonGenericCollectionResolvableTestServiceImpl1<object>),
                        };

            Assert.True(resolvedTypes.SequenceEqual(types));

            Assert.True(DependencyContainer
                       .ResolveCollection<ISingletonGenericCollectionResolvableTestService<object>>()
                       .SequenceEqual(DependencyContainer.ResolveCollection<ISingletonGenericCollectionResolvableTestService<object>>()));
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
                Assert.True(resolved.GetType() == type);
                Output.WriteLine(type.Name);

                if (types.TryGetValue(type, out var nextDecorateeType))
                {
                    var decorator = (IConditionalDecorableServiceDecorator)resolved;
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
                Assert.True(resolved.GetType() == type);
                Output.WriteLine(type.Name);

                if (types[i].TryGetValue(type, out var nextDecorateeType))
                {
                    var decorator = (ICollectionResolvableConditionDecorableServiceDecorator)resolved;

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