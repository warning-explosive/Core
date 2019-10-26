namespace SpaceEngineers.Core.CompositionRoot.Test
{
    using Extensions;
    using Xunit;
    using Xunit.Abstractions;

    public class TypeExtensionsTest : TestBase
    {
        public TypeExtensionsTest(ITestOutputHelper output)
            : base(output) { }

        [Fact]
        public void IsImplementationOfOpenGenericTest()
        {
            Assert.True(typeof(TestTypeImplementation).IsImplementationOfOpenGeneric(typeof(TestGenericTypeImplementationBase<>)));
            Assert.True(typeof(TestGenericTypeImplementation<object>).IsImplementationOfOpenGeneric(typeof(TestGenericTypeImplementationBase<>)));
            Assert.True(typeof(TestGenericTypeImplementation<object>).IsImplementationOfOpenGeneric(typeof(TestGenericTypeImplementation<>)));
            
            Assert.False(typeof(TestTypeImplementation).IsImplementationOfOpenGeneric(typeof(TestGenericTypeImplementationBase<object>)));
            Assert.False(typeof(TestTypeImplementation).IsImplementationOfOpenGeneric(typeof(ITestGenericInterface<>)));
            Assert.False(typeof(TestTypeImplementation).IsImplementationOfOpenGeneric(typeof(ITestGenericInterface<object>)));
        }

        [Fact]
        public void IsImplementationOfOpenGenericInterfaceTest()
        {
            Assert.True(typeof(TestTypeImplementation).IsImplementationOfOpenGenericInterface(typeof(ITestGenericInterface<>)));
            Assert.True(typeof(TestTypeImplementation).IsImplementationOfOpenGenericInterface(typeof(ITestGenericInterfaceBase<>)));
            
            Assert.False(typeof(TestTypeImplementation).IsImplementationOfOpenGenericInterface(typeof(ITestGenericInterface<object>)));
            Assert.False(typeof(TestTypeImplementation).IsImplementationOfOpenGenericInterface(typeof(ITestGenericInterfaceBase<object>)));
            Assert.False(typeof(TestTypeImplementation).IsImplementationOfOpenGenericInterface(typeof(ITestInterface)));
            
            Assert.False(typeof(TestTypeImplementation).IsImplementationOfOpenGenericInterface(typeof(TestGenericTypeImplementationBase<>)));
            Assert.False(typeof(TestTypeImplementation).IsImplementationOfOpenGenericInterface(typeof(TestGenericTypeImplementationBase<object>)));
        }

        [Fact]
        public void IsDerivedFromInterfaceTest()
        {
            Assert.True(typeof(TestTypeImplementation).IsDerivedFromInterface(typeof(ITestGenericInterface<object>)));
            Assert.True(typeof(TestTypeImplementation).IsDerivedFromInterface(typeof(ITestGenericInterfaceBase<object>)));
            Assert.True(typeof(TestTypeImplementation).IsDerivedFromInterface(typeof(ITestInterface)));
            
            Assert.True(typeof(TestGenericTypeImplementation<object>).IsDerivedFromInterface(typeof(ITestGenericInterface<object>)));
            
            Assert.False(typeof(TestTypeImplementation).IsDerivedFromInterface(typeof(ITestGenericInterface<>)));
            Assert.False(typeof(TestTypeImplementation).IsDerivedFromInterface(typeof(TestGenericTypeImplementationBase<>)));
        }

        [Fact]
        public void IsContainsInterfaceDeclarationTest()
        {
            Assert.True(typeof(ITestGenericInterfaceBase<object>).IsContainsInterfaceDeclaration(typeof(ITestInterface)));
            Assert.False(typeof(ITestGenericInterface<object>).IsContainsInterfaceDeclaration(typeof(ITestInterface)));
            
            Assert.False(typeof(ITestInterface).IsContainsInterfaceDeclaration(typeof(ITestInterface)));
            Assert.False(typeof(TestTypeImplementation).IsContainsInterfaceDeclaration(typeof(ITestInterface)));
            
            Assert.False(typeof(TestGenericTypeImplementationBase<object>).IsContainsInterfaceDeclaration(typeof(ITestGenericInterface<string>)));
            Assert.False(typeof(ITestGenericInterface<object>).IsContainsInterfaceDeclaration(typeof(ITestGenericInterfaceBase<string>)));
            
            Assert.True(typeof(TestGenericTypeImplementationBase<object>).IsContainsInterfaceDeclaration(typeof(ITestGenericInterface<object>)));
            Assert.True(typeof(ITestGenericInterface<object>).IsContainsInterfaceDeclaration(typeof(ITestGenericInterfaceBase<object>)));
        }

        private interface ITestInterface { }

        private interface ITestGenericInterfaceBase<T> : ITestInterface { }
        
        private interface ITestGenericInterface<T> : ITestGenericInterfaceBase<T>, ITestInterface { }

        private abstract class TestGenericTypeImplementationBase<T> : ITestGenericInterface<T> { }
        
        private class TestTypeImplementation : TestGenericTypeImplementationBase<object> { }
        
        private class TestGenericTypeImplementation<T> : TestGenericTypeImplementationBase<T> { }
    }
}