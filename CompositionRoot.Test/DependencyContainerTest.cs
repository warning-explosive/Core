namespace SpaceEngineers.Core.CompositionRoot.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Extensions;
    using Xunit;
    using Xunit.Abstractions;

    public class DependencyContainerTest : TestBase
    {
        public DependencyContainerTest(ITestOutputHelper output) : base(output) { }

        [Fact]
        internal void SingletonTest()
        {
            Output.WriteLine(DependencyContainer.Resolve<ISingletonTestService>().Do());
            
            Assert.Equal(DependencyContainer.Resolve<ISingletonTestService>(),
                         DependencyContainer.Resolve<ISingletonTestService>());
        }

        [Fact]
        internal void TypedUntypedSingletonTest()
        {
            Output.WriteLine(((ISingletonTestService)DependencyContainer.Resolve(typeof(ISingletonTestService))).Do());
            
            Assert.Equal(DependencyContainer.Resolve<ISingletonTestService>(),
                         DependencyContainer.Resolve(typeof(ISingletonTestService)));
        }
        
        [Fact]
        internal void OpenGenericTest()
        {
            Output.WriteLine(DependencyContainer.Resolve<IOpenGenericTestService<string>>().Do(nameof(OpenGenericTest)));
            Assert.Equal(nameof(OpenGenericTest), DependencyContainer.Resolve<IOpenGenericTestService<string>>().Do(nameof(OpenGenericTest)));
        }
        
        [Fact]
        internal void AutoWiringTest()
        {
            Output.WriteLine(DependencyContainer.Resolve<IIndependentTestService>().Do());
            Assert.Equal(nameof(IndependentTestServiceImpl), DependencyContainer.Resolve<IIndependentTestService>().Do());

            Output.WriteLine(DependencyContainer.Resolve<IWiredTestService>().Do());
            Assert.Equal(nameof(WiredTestServiceImpl) + " => " + nameof(IndependentTestServiceImpl), DependencyContainer.Resolve<IWiredTestService>().Do());
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
                    var decorator = resolved as IDecorableServiceDecorator;
                    
                    CheckRecursive(decorator.ThrowIfNull().Decoratee, nextDecorateeType);
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
                    var decorator = resolved as IOpenGenericDecorableServiceDecorator<object>;
                    
                    CheckRecursive(decorator.ThrowIfNull().Decoratee, nextDecorateeType);
                }
            }

            CheckRecursive(service, typeof(OpenGenericDecorableServiceDecorator1<object>));
        }
    }
}