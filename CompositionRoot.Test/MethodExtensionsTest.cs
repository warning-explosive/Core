namespace SpaceEngineers.Core.CompositionRoot.Test
{
    using System;
    using System.Linq;
    using System.Reflection;
    using Extensions;
    using Xunit;
    using Xunit.Abstractions;

    public class MethodExtensionsTest : TestBase
    {
        public MethodExtensionsTest(ITestOutputHelper output) : base(output) { }
        
        [Fact]
        public void CallStaticMethodTest()
        {
            Assert.True((bool)typeof(TestType).CallStaticMethod(nameof(TestType.PublicStaticMethod)));
            Assert.True((bool)typeof(TestType).CallStaticMethod("PrivateStaticMethod"));
            
            Assert.True((bool)typeof(TestType).CallStaticMethod(nameof(TestType.PublicStaticMethodWithArgs), true));
            Assert.True((bool)typeof(TestType).CallStaticMethod("PrivateStaticMethodWithArgs", true));
            
            Assert.True((bool)typeof(TestType).CallStaticMethod(nameof(TestType.PublicStaticMethodWithSeveralArgs), true, true));
            Assert.True((bool)typeof(TestType).CallStaticMethod(nameof(TestType.PublicStaticMethodWithSeveralArgs), true, true, true));
            Assert.True((bool)typeof(TestType).CallStaticMethod("PrivateStaticMethodWithSeveralArgs", true, true));

            Assert.Throws<InvalidOperationException>(() => typeof(TestType).CallStaticMethod(nameof(TestType.PublicStaticMethodWithParams), new object[] { true, true, true }));
            Assert.Throws<InvalidOperationException>(() => typeof(TestType).CallStaticMethod("PrivateStaticMethodWithParams", new object[] { true, true, true }));
            
            Assert.True((bool)typeof(TestType).CallStaticMethod(nameof(TestType.PublicStaticMethodWithParams), new object[] { new object[] { true, true, true } }));
            Assert.True((bool)typeof(TestType).CallStaticMethod("PrivateStaticMethodWithParams", new object[] { new object[] { true, true, true } }));
        }
        
        [Fact]
        public void CallStaticGenericMethodTest()
        {
            Assert.True((bool)typeof(TestType).CallStaticGenericMethod("PrivateStaticGenericMethod", new[] { typeof(bool) }, true));
        }
        
        private class TestType
        {
            public static bool PublicStaticMethod() => true;

            private static bool PrivateStaticMethod() => true;

            public static bool PublicStaticMethodWithArgs(bool flag) => flag;

            private static bool PrivateStaticMethodWithArgs(bool flag) => flag;

            public static bool PublicStaticMethodWithSeveralArgs(bool flag1, bool flag2) => flag1 && flag2;
            
            public static bool PublicStaticMethodWithSeveralArgs(bool flag1, bool flag2, bool flag3) => flag1 && flag2 && flag3;

            private static bool PrivateStaticMethodWithSeveralArgs(bool flag1, bool flag2) => flag1 && flag2;

            public static bool PublicStaticMethodWithParams(params object[] flags) => flags.OfType<bool>().All(z => z);

            private static bool PrivateStaticMethodWithParams(params object[] flags) => flags.OfType<bool>().All(z => z);
            
            private static T PrivateStaticGenericMethod<T>(T flag) => flag;
        }
    }
}