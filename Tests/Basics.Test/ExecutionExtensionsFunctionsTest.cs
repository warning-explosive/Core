namespace SpaceEngineers.Core.Basics.Test
{
    using System;
    using Basics;
    using Xunit;
    using Xunit.Abstractions;
    using Xunit.Sdk;

    /// <summary>
    /// ExecutionExtensions class tests
    /// </summary>
    public class ExecutionExtensionsFunctionsTest : BasicsTestBase
    {
        /// <summary> .ctor </summary>
        /// <param name="output">ITestOutputHelper</param>
        public ExecutionExtensionsFunctionsTest(ITestOutputHelper output)
            : base(output)
        {
        }

        [Fact]
        internal void HandleCaughtExceptionsTest()
        {
            Func<bool> func = () => throw TestExtensions.TrueException();

            var emptyHandlerBlockResult = ExecutionExtensions
                .Try(func)
                .Catch<TrueException>()
                .Invoke(_ => default);

            Assert.False(emptyHandlerBlockResult);

            emptyHandlerBlockResult = ExecutionExtensions
                .Try(func)
                .Catch<TrueException>(_ => { })
                .Invoke(_ => default);

            Assert.False(emptyHandlerBlockResult);

            var result = ExecutionExtensions
                .Try(func)
                .Catch<TrueException>()
                .Invoke(_ => true);

            Assert.True(result);

            void HandleNotCaught() => ExecutionExtensions
                .Try(func)
                .Catch<FalseException>()
                .Invoke(_ => true);

            Assert.Throws<TrueException>(HandleNotCaught);
        }

        [Fact]
        internal void SimpleTest()
        {
            // 1 - value type
            Func<bool> valueTypeFunction = () => true;
            var result1 = ExecutionExtensions.Try(valueTypeFunction).Invoke(_ => default);
            Assert.True(result1);

            // 2 - nullable value type
            Func<bool?> nullableValueTypeFunction = () => null;
            var result2 = ExecutionExtensions.Try(nullableValueTypeFunction).Invoke(_ => default);
            Assert.Null(result2);

            // 3 - reference type
            Func<object> referenceFunction = () => new object();
            var result3 = ExecutionExtensions.Try(referenceFunction).Invoke(_ => new object());
            Assert.NotNull(result3);

            // nullable-reference type
            Func<object?> nullableReferenceFunction = () => null;
            var result4 = ExecutionExtensions.Try(nullableReferenceFunction).Invoke(_ => default);
            Assert.Null(result4);
        }

        [Fact]
        internal void HandledExceptionTest()
        {
            Func<object?> function = () => throw TestExtensions.FalseException();

            ExecutionExtensions
                .Try(function)
                .Catch<FalseException>()
                .Invoke(_ => default);
        }

        [Fact]
        internal void SeveralCatchBlocksTest()
        {
            Func<object?> function = () => throw TestExtensions.FalseException();

            ExecutionExtensions
                .Try(function)
                .Catch<TrueException>(ex => throw ex)
                .Catch<FalseException>()
                .Invoke(_ => default);
        }

        [Fact]
        internal void ThrowInCatchBlockTest()
        {
            Func<object?> function = () => throw TestExtensions.FalseException();

            void TestFunction() => ExecutionExtensions
                .Try(function)
                .Catch<FalseException>(_ => throw TestExtensions.TrueException())
                .Invoke(_ => default);

            Assert.Throws<TrueException>(TestFunction);
        }

        [Fact]
        internal void ThrowInInvokeBlockTest()
        {
            Func<object> function = () => throw TestExtensions.FalseException();

            void TestFunction() => ExecutionExtensions
                .Try(function)
                .Catch<FalseException>()
                .Invoke(ex => throw TestExtensions.TrueException());

            Assert.Throws<TrueException>(TestFunction);
        }

        [Fact]
        internal void ThrowInFinallyBlockTest()
        {
            Func<object?> function = () => throw TestExtensions.FalseException();

            void TestFunction() => ExecutionExtensions
                .Try(function)
                .Catch<FalseException>(ex => throw ex)
                .Finally(() => throw TestExtensions.TrueException())
                .Invoke(_ => default);

            Assert.Throws<TrueException>(TestFunction);
        }
    }
}