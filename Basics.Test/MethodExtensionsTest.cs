namespace SpaceEngineers.Core.Basics.Test
{
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Basics;
    using Exceptions;
    using Xunit;
    using Xunit.Abstractions;
    using Xunit.Sdk;

    /// <summary>
    /// MethodExtensions class tests
    /// </summary>
    public class MethodExtensionsTest : BasicsTestBase
    {
        /// <summary> .ctor </summary>
        /// <param name="output">ITestOutputHelper</param>
        public MethodExtensionsTest(ITestOutputHelper output)
            : base(output) { }

        [Fact]
        internal void CallStaticMethodTest()
        {
            Assert.True(typeof(StaticTestClass).CallMethod(nameof(StaticTestClass.PublicStaticMethod)).Invoke<bool>());
            Assert.True(typeof(StaticTestClass).CallMethod("PrivateStaticMethod").Invoke<bool>());

            Assert.True(typeof(StaticTestClass).CallMethod(nameof(StaticTestClass.PublicStaticMethodWithArgs)).WithArgument(true).Invoke<bool>());
            Assert.True(typeof(StaticTestClass).CallMethod("PrivateStaticMethodWithArgs").WithArgument(true).Invoke<bool>());

            Assert.True(typeof(StaticTestClass).CallMethod(nameof(StaticTestClass.PublicStaticMethodWithSeveralArgs)).WithArgument(true).WithArgument(true).Invoke<bool>());
            Assert.True(typeof(StaticTestClass).CallMethod(nameof(StaticTestClass.PublicStaticMethodWithSeveralArgs)).WithArgument(true).WithArgument(true).WithArgument(true).Invoke<bool>());
            Assert.True(typeof(StaticTestClass).CallMethod("PrivateStaticMethodWithSeveralArgs").WithArgument(true).WithArgument(true).Invoke<bool>());

            _ = Assert.Throws<NotFoundException>(() => typeof(StaticTestClass).CallMethod(nameof(StaticTestClass.PublicStaticMethodWithParams)).WithArgument(true).WithArgument(true).WithArgument(true).Invoke());
            _ = Assert.Throws<NotFoundException>(() => typeof(StaticTestClass).CallMethod("PrivateStaticMethodWithParams").WithArgument(true).WithArgument(true).WithArgument(true).Invoke());

            Assert.True(typeof(StaticTestClass).CallMethod(nameof(StaticTestClass.PublicStaticMethodWithParams)).WithArgument(new object[] { true, true, true }).Invoke<bool>());
            Assert.True(typeof(StaticTestClass).CallMethod("PrivateStaticMethodWithParams").WithArgument(new object[] { true, true, true }).Invoke<bool>());
        }

        [Fact]
        internal void CallInstanceMethodTest()
        {
            var target = new InstanceTestClass();

            Assert.True(target.CallMethod(nameof(InstanceTestClass.PublicMethod)).Invoke<bool>());
            Assert.True(target.CallMethod("PrivateMethod").Invoke<bool>());

            Assert.True(target.CallMethod(nameof(InstanceTestClass.PublicMethodWithArgs)).WithArgument(true).Invoke<bool>());
            Assert.True(target.CallMethod("PrivateMethodWithArgs").WithArgument(true).Invoke<bool>());

            Assert.True(target.CallMethod(nameof(InstanceTestClass.PublicMethodWithSeveralArgs)).WithArgument(true).WithArgument(true).Invoke<bool>());
            Assert.True(target.CallMethod(nameof(InstanceTestClass.PublicMethodWithSeveralArgs)).WithArgument(true).WithArgument(true).WithArgument(true).Invoke<bool>());
            Assert.True(target.CallMethod("PrivateMethodWithSeveralArgs").WithArgument(true).WithArgument(true).Invoke<bool>());

            _ = Assert.Throws<NotFoundException>(() => target.CallMethod(nameof(InstanceTestClass.PublicMethodWithParams)).WithArgument(true).WithArgument(true).WithArgument(true).Invoke());
            _ = Assert.Throws<NotFoundException>(() => target.CallMethod("PrivateMethodWithParams").WithArgument(true).WithArgument(true).WithArgument(true).Invoke());

            Assert.True(target.CallMethod(nameof(InstanceTestClass.PublicMethodWithParams)).WithArgument(new object[] { true, true, true }).Invoke<bool>());
            Assert.True(target.CallMethod("PrivateMethodWithParams").WithArgument(new object[] { true, true, true }).Invoke<bool>());
        }

        [Fact]
        internal void CallStaticGenericMethodTest()
        {
            // 1 - close on reference
            Assert.Throws<TrueException>(() => typeof(StaticTestClass).CallMethod(nameof(StaticTestClass.PublicStaticGenericMethod)).WithTypeArgument<object>().Invoke());
            Assert.Throws<TrueException>(() => typeof(StaticTestClass).CallMethod("PrivateStaticGenericMethod").WithTypeArgument<object>().Invoke());

            Assert.Throws<FalseException>(() => typeof(StaticTestClass).CallMethod(nameof(StaticTestClass.PublicStaticGenericMethod)).WithTypeArgument<object>().WithTypeArgument<object>().Invoke());
            Assert.Throws<FalseException>(() => typeof(StaticTestClass).CallMethod("PrivateStaticGenericMethod").WithTypeArgument<object>().WithTypeArgument<object>().Invoke());

            // 2 - close on value
            Assert.True(typeof(StaticTestClass).CallMethod(nameof(StaticTestClass.PublicStaticGenericMethod)).WithTypeArgument<object>().WithArgument(true).Invoke<bool>());
            Assert.True(typeof(StaticTestClass).CallMethod("PrivateStaticGenericMethod").WithTypeArgument<bool>().WithArgument(true).Invoke<bool>());

            Assert.True(typeof(StaticTestClass).CallMethod(nameof(StaticTestClass.PublicStaticGenericMethod)).WithTypeArgument(typeof(bool)).WithArgument(true).Invoke<bool>());
            Assert.True(typeof(StaticTestClass).CallMethod("PrivateStaticGenericMethod").WithTypeArgument(typeof(bool)).WithArgument(true).Invoke<bool>());

            // 3 - with generic input - reference
            Assert.NotNull(typeof(StaticTestClass).CallMethod(nameof(StaticTestClass.PublicStaticGenericMethod)).WithTypeArgument<object>().WithArgument(new object()).Invoke<object>());
            Assert.NotNull(typeof(StaticTestClass).CallMethod("PrivateStaticGenericMethod").WithTypeArgument<object>().WithArgument(new object()).Invoke<object>());

            // 4 - with generic input - value
            Assert.True(typeof(StaticTestClass).CallMethod(nameof(StaticTestClass.PublicStaticGenericMethod)).WithTypeArgument<bool>().WithArgument(true).Invoke<bool>());
            Assert.True(typeof(StaticTestClass).CallMethod("PrivateStaticGenericMethod").WithTypeArgument<bool>().WithArgument(true).Invoke<bool>());
        }

        [Fact]
        internal void CallInstanceGenericMethodTest()
        {
            var target = new InstanceTestClass();

            // 1 - close on reference
            Assert.Throws<TrueException>(() => target.CallMethod(nameof(InstanceTestClass.PublicGenericMethod)).WithTypeArgument<object>().Invoke());
            Assert.Throws<TrueException>(() => target.CallMethod("PrivateGenericMethod").WithTypeArgument<object>().Invoke());

            Assert.Throws<FalseException>(() => target.CallMethod(nameof(InstanceTestClass.PublicGenericMethod)).WithTypeArgument<object>().WithTypeArgument<object>().Invoke());
            Assert.Throws<FalseException>(() => target.CallMethod("PrivateGenericMethod").WithTypeArgument<object>().WithTypeArgument<object>().Invoke());

            // 2 - close on value
            Assert.True(target.CallMethod(nameof(InstanceTestClass.PublicGenericMethod)).WithTypeArgument<bool>().WithArgument(true).Invoke<bool>());
            Assert.True(target.CallMethod("PrivateGenericMethod").WithTypeArgument<bool>().WithArgument(true).Invoke<bool>());

            Assert.True(target.CallMethod(nameof(InstanceTestClass.PublicGenericMethod)).WithTypeArgument(typeof(bool)).WithArgument(true).Invoke<bool>());
            Assert.True(target.CallMethod("PrivateGenericMethod").WithTypeArgument(typeof(bool)).WithArgument(true).Invoke<bool>());

            // 3 - with generic input - reference
            Assert.NotNull(target.CallMethod(nameof(InstanceTestClass.PublicGenericMethod)).WithTypeArgument<object>().WithArgument(new object()).Invoke<object>());
            Assert.NotNull(target.CallMethod("PrivateGenericMethod").WithTypeArgument<object>().WithArgument(new object()).Invoke<object>());

            // 4 - with generic input - value
            Assert.True(target.CallMethod(nameof(InstanceTestClass.PublicGenericMethod)).WithTypeArgument<bool>().WithArgument(true).Invoke<bool>());
            Assert.True(target.CallMethod("PrivateGenericMethod").WithTypeArgument<bool>().WithArgument(true).Invoke<bool>());
        }

        [Fact]
        internal void NullArgumentTest()
        {
            var target = new NullableTestClass();

            Assert.Throws<TrueException>(() => target.CallMethod(nameof(NullableTestClass.MethodWithNullArgs)).WithArgument<object?>(null).Invoke());
            Assert.Throws<TrueException>(() => target.CallMethod(nameof(NullableTestClass.MethodWithOptionalNullArgs)).WithArgument<object?>(null).Invoke());
        }

        [Fact]
        internal void InheritanceCallTest()
        {
            var baseTarget = new TestClassBase();
            var derivedTarget = new DerivedClass();

            Assert.Throws<FalseException>(() => baseTarget.CallMethod(nameof(TestClassBase.BaseMethod)).Invoke());
            Assert.Throws<FalseException>(() => baseTarget.CallMethod("VirtualMethod").Invoke());
            Assert.Throws<FalseException>(() => derivedTarget.CallMethod(nameof(DerivedClass.BaseMethod)).Invoke());

            Assert.Throws<TrueException>(() => derivedTarget.CallMethod(nameof(DerivedClass.DerivedMethod)).Invoke());
            Assert.Throws<TrueException>(() => derivedTarget.CallMethod("VirtualMethod").Invoke());
        }

        [SuppressMessage("StyleCop.Analyzers", "SA1202", Justification = "For test reasons")]
        [SuppressMessage("Microsoft.CodeAnalysis.CSharp.Features", "IDE0051", Justification = "For test reasons")]
        private class StaticTestClass
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

            public static T PublicStaticGenericMethod<T>() => throw TestExtensions.TrueException();

            private static T PrivateStaticGenericMethod<T>() => throw TestExtensions.TrueException();

            public static T1 PublicStaticGenericMethod<T1, T2>() => throw TestExtensions.FalseException();

            private static T1 PrivateStaticGenericMethod<T1, T2>() => throw TestExtensions.FalseException();

            public static bool PublicStaticGenericMethod<T>(bool flag) => flag;

            private static bool PrivateStaticGenericMethod<T>(bool flag) => flag;

            public static T PublicStaticGenericMethod<T>(T flag) => flag;

            private static T PrivateStaticGenericMethod<T>(T flag) => flag;
        }

        [SuppressMessage("StyleCop.Analyzers", "SA1202", Justification = "For test reasons")]
        [SuppressMessage("Microsoft.CodeAnalysis.CSharp.Features", "IDE0051", Justification = "For test reasons")]
        private class InstanceTestClass
        {
            internal bool PublicMethod() => true;

            private bool PrivateMethod() => true;

            internal bool PublicMethodWithArgs(bool flag) => flag;

            private bool PrivateMethodWithArgs(bool flag) => flag;

            internal bool PublicMethodWithSeveralArgs(bool flag1, bool flag2) => flag1 && flag2;

            internal bool PublicMethodWithSeveralArgs(bool flag1, bool flag2, bool flag3) => flag1 && flag2 && flag3;

            private bool PrivateMethodWithSeveralArgs(bool flag1, bool flag2) => flag1 && flag2;

            internal bool PublicMethodWithParams(params object[] flags) => flags.OfType<bool>().All(z => z);

            private bool PrivateMethodWithParams(params object[] flags) => flags.OfType<bool>().All(z => z);

            public T PublicGenericMethod<T>() => throw TestExtensions.TrueException();

            private T PrivateGenericMethod<T>() => throw TestExtensions.TrueException();

            public T1 PublicGenericMethod<T1, T2>() => throw TestExtensions.FalseException();

            private T1 PrivateGenericMethod<T1, T2>() => throw TestExtensions.FalseException();

            public bool PublicGenericMethod<T>(bool flag) => flag;

            private bool PrivateGenericMethod<T>(bool flag) => flag;

            public T PublicGenericMethod<T>(T input) => input;

            private T PrivateGenericMethod<T>(T input) => input;
        }

        private class NullableTestClass
        {
            public void MethodWithNullArgs(object? arg)
            {
                if (arg == null)
                {
                    throw TestExtensions.TrueException();
                }
            }

            public void MethodWithOptionalNullArgs(object? arg = null)
            {
                if (arg == null)
                {
                    throw TestExtensions.TrueException();
                }
            }
        }

        private class TestClassBase
        {
            public void BaseMethod()
            {
                throw TestExtensions.FalseException();
            }

            protected virtual void VirtualMethod()
            {
                throw TestExtensions.FalseException();
            }
        }

        private class DerivedClass : TestClassBase
        {
            public void DerivedMethod()
            {
                throw TestExtensions.TrueException();
            }

            protected override void VirtualMethod()
            {
                throw TestExtensions.TrueException();
            }
        }
    }
}