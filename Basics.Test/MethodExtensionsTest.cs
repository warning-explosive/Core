namespace SpaceEngineers.Core.Basics.Test
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Basics;
    using Xunit;
    using Xunit.Abstractions;

    public class MethodExtensionsTest : BasicsTestBase
    {
        public MethodExtensionsTest(ITestOutputHelper output)
            : base(output) { }

        [Fact]
        internal void CallStaticMethodTest()
        {
            Assert.True(typeof(TestType).CallStaticMethod(nameof(TestType.PublicStaticMethod)).ExtractNotNullableSafely<bool>());

            Assert.True(typeof(TestType).CallStaticMethod("PrivateStaticMethod").ExtractNotNullableSafely<bool>());

            Assert.True(typeof(TestType).CallStaticMethod(nameof(TestType.PublicStaticMethodWithArgs), true).ExtractNotNullableSafely<bool>());
            Assert.True(typeof(TestType).CallStaticMethod("PrivateStaticMethodWithArgs", true).ExtractNotNullableSafely<bool>());

            Assert.True(typeof(TestType).CallStaticMethod(nameof(TestType.PublicStaticMethodWithSeveralArgs), true, true).ExtractNotNullableSafely<bool>());
            Assert.True(typeof(TestType).CallStaticMethod(nameof(TestType.PublicStaticMethodWithSeveralArgs), true, true, true).ExtractNotNullableSafely<bool>());
            Assert.True(typeof(TestType).CallStaticMethod("PrivateStaticMethodWithSeveralArgs", true, true).ExtractNotNullableSafely<bool>());

            _ = Assert.Throws<InvalidOperationException>(() => typeof(TestType).CallStaticMethod(nameof(TestType.PublicStaticMethodWithParams), new object[] { true, true, true }));
            _ = Assert.Throws<InvalidOperationException>(() => typeof(TestType).CallStaticMethod("PrivateStaticMethodWithParams", new object[] { true, true, true }));

            Assert.True(typeof(TestType).CallStaticMethod(nameof(TestType.PublicStaticMethodWithParams), new object[] { new object[] { true, true, true } }).ExtractNotNullableSafely<bool>());
            Assert.True(typeof(TestType).CallStaticMethod("PrivateStaticMethodWithParams", new object[] { new object[] { true, true, true } }).ExtractNotNullableSafely<bool>());
        }

        [Fact]
        internal void CallStaticGenericMethodTest()
        {
            Assert.True(typeof(TestType).CallStaticGenericMethod("PrivateStaticGenericMethod", new[] { typeof(bool) }, true).ExtractNotNullableSafely<bool>());
        }

        [SuppressMessage("StyleCop.Analyzers", "SA1202", Justification = "For test reasons")]
        [SuppressMessage("Microsoft.CodeAnalysis.CSharp.Features", "IDE0051", Justification = "For test reasons")]
        private class TestType
        {
            internal static bool PublicStaticMethod() => true;

            private static bool PrivateStaticMethod() => true;

            internal static bool PublicStaticMethodWithArgs(bool flag) => flag;

            private static bool PrivateStaticMethodWithArgs(bool flag) => flag;

            internal static bool PublicStaticMethodWithSeveralArgs(bool flag1, bool flag2) => flag1 && flag2;

            internal static bool PublicStaticMethodWithSeveralArgs(bool flag1, bool flag2, bool flag3) => flag1 && flag2 && flag3;

            private static bool PrivateStaticMethodWithSeveralArgs(bool flag1, bool flag2) => flag1 && flag2;

            internal static bool PublicStaticMethodWithParams(params object[] flags) => flags.OfType<bool>().All(z => z);

            private static bool PrivateStaticMethodWithParams(params object[] flags) => flags.OfType<bool>().All(z => z);

            private static T PrivateStaticGenericMethod<T>(T flag) => flag;
        }
    }
}