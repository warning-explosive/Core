namespace SpaceEngineers.Core.Utilities.Test.Extensions
{
    using System;
    using Utilities.Extensions;
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
        }

        [Fact]
        public void IsImplementationOfOpenGenericInterfaceTest()
        {
            Assert.True(typeof(TestGenericTypeImplementation).IsImplementationOfOpenGenericInterface(typeof(ITetsGenericInterface<>)));
        }
        
        private interface ITetsGenericInterface<T> { }

        private abstract class TestGenericTypeImplementationBase<T> : ITetsGenericInterface<T> { }
        
        private class TestGenericTypeImplementation : TestGenericTypeImplementationBase<object> { }
    }
}