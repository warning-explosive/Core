namespace SpaceEngineers.Core.CompositionRoot.Test
{
    using System;
    using System.Linq;
    using Abstractions;
    using Xunit;
    using Xunit.Abstractions;

    public class DependencyContainerTest : TestBase
    {
        public DependencyContainerTest(ITestOutputHelper output) : base(output) { }

        [Fact]
        public void SingletonTest()
        {
            Output.WriteLine(DependencyContainer.Resolve<ISingletonTestService>().Do());
            
            Assert.Equal(DependencyContainer.Resolve<ISingletonTestService>(),
                         DependencyContainer.Resolve<ISingletonTestService>());
        }

        [Fact]
        public void TypedUntypedSingletonTest()
        {
            Output.WriteLine(((ISingletonTestService)DependencyContainer.Resolve(typeof(ISingletonTestService))).Do());
            
            Assert.Equal(DependencyContainer.Resolve<ISingletonTestService>(),
                         DependencyContainer.Resolve(typeof(ISingletonTestService)));
        }
        
        [Fact]
        public void OpenGenericTest()
        {
            Output.WriteLine(DependencyContainer.Resolve<IOpenGenericTestService<string>>().Do(nameof(OpenGenericTest)));
            Assert.Equal(nameof(OpenGenericTest), DependencyContainer.Resolve<IOpenGenericTestService<string>>().Do(nameof(OpenGenericTest)));
        }
        
        [Fact]
        public void AutoWiringTest()
        {
            Output.WriteLine(DependencyContainer.Resolve<IIndependentTestService>().Do());
            Assert.Equal(nameof(IndependentTestServiceImpl), DependencyContainer.Resolve<IIndependentTestService>().Do());

            Output.WriteLine(DependencyContainer.Resolve<IWiredTestService>().Do());
            Assert.Equal(nameof(WiredTestServiceImpl) + " => " + nameof(IndependentTestServiceImpl), DependencyContainer.Resolve<IWiredTestService>().Do());
        }

        [Fact]
        public void OrderedCollectionResolvableTest()
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
        public void UntypedOrderedCollectionResolvableTest()
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
        public void SingletonOpenGenericCollectionResolvableTest()
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
    }
}