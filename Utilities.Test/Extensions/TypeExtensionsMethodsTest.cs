namespace SpaceEngineers.Core.Utilities.Test.Extensions
{
    using System.Linq;
    using System.Reflection;
    using Utilities.Extensions;
    using Xunit;
    using Xunit.Abstractions;

    public class TypeExtensionsMethodsTest : TestBase
    {
        public TypeExtensionsMethodsTest(ITestOutputHelper output)
            : base(output) { }
        
        [Fact]
        public void CallStaticMethodTest()
        {
            Assert.True((bool)typeof(TestType).CallStaticMethod(nameof(TestType.PublicStaticMethod)));
            Assert.True((bool)typeof(TestType).CallStaticMethod("PrivateStaticMethod"));
            
            Assert.True((bool)typeof(TestType).CallStaticMethod(nameof(TestType.PublicStaticMethodWithArgs), true));
            Assert.True((bool)typeof(TestType).CallStaticMethod("PrivateStaticMethodWithArgs", true));
            
            Assert.True((bool)typeof(TestType).CallStaticMethod(nameof(TestType.PublicStaticMethodWithSeveralArgs), true, true));
            Assert.True((bool)typeof(TestType).CallStaticMethod("PrivateStaticMethodWithSeveralArgs", true, true));

            Assert.Throws<TargetParameterCountException>(() => typeof(TestType).CallStaticMethod(nameof(TestType.PublicStaticMethodWithParams), true, true, true));
            Assert.Throws<TargetParameterCountException>(() => typeof(TestType).CallStaticMethod("PrivateStaticMethodWithParams", true, true, true));
        }
        
        private class TestType
        {
            public static bool PublicStaticMethod() => true;

            private static bool PrivateStaticMethod() => true;

            public static bool PublicStaticMethodWithArgs(bool flag) => flag;

            private static bool PrivateStaticMethodWithArgs(bool flag) => flag;

            public static bool PublicStaticMethodWithSeveralArgs(bool flag1, bool flag2) => flag1 && flag2;

            private static bool PrivateStaticMethodWithSeveralArgs(bool flag1, bool flag2) => flag1 && flag2;

            public static bool PublicStaticMethodWithParams(params object[] flags) => flags.OfType<bool>().All(z => z);

            private static bool PrivateStaticMethodWithParams(params object[] flags) => flags.OfType<bool>().All(z => z);
        }
    }
}