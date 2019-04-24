namespace SpaceEngineers.Core.Utilities.Test.Services.DependencyContainerTests
{
    using System;
    using Utilities.Services.Implementations;
    using Utilities.Services.Interfaces;
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
    }
}