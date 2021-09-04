namespace SpaceEngineers.Core.Basics.Test
{
    using System;
    using System.Globalization;
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

            var emptyHandlerBlockResult = func.Try()
                                              .Catch<TrueException>()
                                              .Invoke();
            Assert.False(emptyHandlerBlockResult);

            emptyHandlerBlockResult = func.Try()
                                          .Catch<TrueException>(ex => { })
                                          .Invoke();
            Assert.False(emptyHandlerBlockResult);

            var result = func.Try()
                             .Catch<TrueException>()
                             .Invoke(ex => true);
            Assert.True(result);

            void HandleNotCaught() => func.Try()
                                          .Catch<FalseException>()
                                          .Invoke(ex => true);

            Assert.Throws<TrueException>(HandleNotCaught);
        }

        [Fact]
        internal void WrongNullableInferenceTest()
        {
            // TODO: #38 - Compiler Issue
            object? x = null;
            void Action() => Output.WriteLine(x.GetHashCode().ToString(CultureInfo.InvariantCulture));
            Assert.Throws<NullReferenceException>(Action);
        }

        [Fact]
        internal void SimpleTest()
        {
            // 1 - value type
            Func<bool> valueTypeFunction = () => true;
            var result1 = valueTypeFunction.Try().Invoke();
            Assert.True(result1);

            // 2 - nullable value type
            Func<bool?> nullableValueTypeFunction = () => null;
            var result2 = nullableValueTypeFunction.Try().Invoke();
            Assert.Null(result2);

            // 3 - reference type
            Func<object> referenceFunction = () => new object();
            var result3 = referenceFunction.Try().Invoke();
            Assert.NotNull(result3);

            // nullable-reference type
            Func<object?> nullableReferenceFunction = () => null;
            var result4 = nullableReferenceFunction.Try().Invoke();
            Assert.Null(result4);
        }

        [Fact]
        internal void HandledExceptionTest()
        {
            Func<object> function = () => throw TestExtensions.FalseException();

            function.Try()
                    .Catch<FalseException>()
                    .Invoke();
        }

        [Fact]
        internal void SeveralCatchBlocksTest()
        {
            Func<object> function = () => throw TestExtensions.FalseException();

            function.Try()
                    .Catch<TrueException>(ex => throw ex)
                    .Catch<FalseException>()
                    .Invoke();
        }

        [Fact]
        internal void ThrowInCatchBlockTest()
        {
            Func<object> function = () => throw TestExtensions.FalseException();

            void TestFunction() => function.Try()
                                           .Catch<FalseException>(ex => throw TestExtensions.TrueException())
                                           .Invoke();

            Assert.Throws<TrueException>(TestFunction);
        }

        [Fact]
        internal void ThrowInInvokeBlockTest()
        {
            Func<object> function = () => throw TestExtensions.FalseException();

            void TestFunction() => function.Try()
                                           .Catch<FalseException>()
                                           .Invoke(ex => throw TestExtensions.TrueException());

            Assert.Throws<TrueException>(TestFunction);
        }

        [Fact]
        internal void ThrowInFinallyBlockTest()
        {
            Func<object> function = () => throw TestExtensions.FalseException();

            void TestFunction() => function.Try()
                                           .Catch<FalseException>(ex => throw ex)
                                           .Finally(() => throw TestExtensions.TrueException())
                                           .Invoke();

            Assert.Throws<TrueException>(TestFunction);
        }
    }
}