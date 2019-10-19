namespace SpaceEngineers.Core.CompositionRoot.Test
{
    using System;
    using Extensions;
    using Xunit;
    using Xunit.Abstractions;

    public class TypeExtensionsFeaturesTest : TestBase
    {
        public TypeExtensionsFeaturesTest(ITestOutputHelper output)
            : base(output) { }

        [Fact]
        public void IsImplementationOfOpenGenericTest()
        {
            Assert.True(typeof(bool?).IsImplementationOfOpenGeneric(typeof(Nullable<>)));
            Assert.True(typeof(TestGenericTypeImplementation).IsImplementationOfOpenGeneric(typeof(TestGenericTypeImplementationBase<>)));
            
            Assert.False(typeof(bool?).IsImplementationOfOpenGeneric(typeof(bool?)));
            Assert.False(typeof(TestGenericTypeImplementation).IsImplementationOfOpenGeneric(typeof(TestGenericTypeImplementationBase<object>)));
            
            Assert.False(typeof(TestGenericTypeImplementation).IsImplementationOfOpenGeneric(typeof(ITestGenericInterface<>)));
            Assert.False(typeof(TestGenericTypeImplementation).IsImplementationOfOpenGeneric(typeof(ITestGenericInterface<object>)));
        }

        [Fact]
        public void IsImplementationOfOpenGenericInterfaceTest()
        {
            Assert.True(typeof(TestGenericTypeImplementation).IsImplementationOfOpenGenericInterface(typeof(ITestGenericInterface<>)));
            Assert.True(typeof(TestGenericTypeImplementation).IsImplementationOfOpenGenericInterface(typeof(ITestGenericInterfaceBase<>)));
            
            Assert.False(typeof(TestGenericTypeImplementation).IsImplementationOfOpenGenericInterface(typeof(ITestGenericInterface<object>)));
            Assert.False(typeof(TestGenericTypeImplementation).IsImplementationOfOpenGenericInterface(typeof(ITestGenericInterfaceBase<object>)));
            Assert.False(typeof(TestGenericTypeImplementation).IsImplementationOfOpenGenericInterface(typeof(ITestInterface)));
            
            Assert.False(typeof(TestGenericTypeImplementation).IsImplementationOfOpenGenericInterface(typeof(TestGenericTypeImplementationBase<>)));
            Assert.False(typeof(TestGenericTypeImplementation).IsImplementationOfOpenGenericInterface(typeof(TestGenericTypeImplementationBase<object>)));
        }

        [Fact]
        public void IsDerivedFromInterfaceTest()
        {
            Assert.True(typeof(TestGenericTypeImplementation).IsDerivedFromInterface(typeof(ITestGenericInterface<object>)));
            Assert.True(typeof(TestGenericTypeImplementation).IsDerivedFromInterface(typeof(ITestGenericInterfaceBase<object>)));
            Assert.True(typeof(TestGenericTypeImplementation).IsDerivedFromInterface(typeof(ITestInterface)));
            
            Assert.False(typeof(TestGenericTypeImplementation).IsDerivedFromInterface(typeof(ITestGenericInterface<>)));
            Assert.False(typeof(TestGenericTypeImplementation).IsDerivedFromInterface(typeof(TestGenericTypeImplementationBase<>)));
        }

        [Fact]
        public void IsContainsInterfaceDeclarationTest()
        {
            foreach (var i in typeof(TestGenericTypeImplementationBase<object>).GetInterfaces())
            {
                Output.WriteLine(i.Name);
            }
            
            Assert.True(typeof(TestGenericTypeImplementationBase<object>).IsContainsInterfaceDeclaration(typeof(ITestGenericInterface<object>)));
            Assert.True(typeof(ITestGenericInterface<object>).IsContainsInterfaceDeclaration(typeof(ITestGenericInterfaceBase<object>)));
            
            Assert.True(typeof(ITestGenericInterfaceBase<object>).IsContainsInterfaceDeclaration(typeof(ITestInterface)));
            Assert.False(typeof(ITestGenericInterface<object>).IsContainsInterfaceDeclaration(typeof(ITestInterface)));
            
            Assert.False(typeof(ITestInterface).IsContainsInterfaceDeclaration(typeof(ITestInterface)));
            Assert.False(typeof(TestGenericTypeImplementation).IsContainsInterfaceDeclaration(typeof(ITestInterface)));
        }

        private interface ITestInterface { }

        private interface ITestGenericInterfaceBase<T> : ITestInterface { }
        
        private interface ITestGenericInterface<T> : ITestGenericInterfaceBase<T>, ITestInterface { }

        private abstract class TestGenericTypeImplementationBase<T> : ITestGenericInterface<T> { }
        
        private class TestGenericTypeImplementation : TestGenericTypeImplementationBase<object> { }
    }
}