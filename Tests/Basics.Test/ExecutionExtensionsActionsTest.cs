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
    public class ExecutionExtensionsActionsTest : BasicsTestBase
    {
        /// <summary> .ctor </summary>
        /// <param name="output">ITestOutputHelper</param>
        public ExecutionExtensionsActionsTest(ITestOutputHelper output)
            : base(output)
        {
        }

        [Fact]
        internal void HandleCaughtExceptionsTest()
        {
            Action action = () => throw TestExtensions.TrueException();

            ExecutionExtensions.Try(action).Catch<TrueException>().Invoke();
            ExecutionExtensions.Try(action).Catch<TrueException>(ex => { }).Invoke();

            void HandleCaught() => ExecutionExtensions
                .Try(action)
                .Catch<TrueException>(ex => throw ex)
                .Invoke();

            Assert.Throws<TrueException>(HandleCaught);

            void Rethrow() => ExecutionExtensions
                .Try(action)
                .Catch<TrueException>(ex => throw TestExtensions.FalseException())
                .Invoke();

            Assert.Throws<FalseException>(Rethrow);

            void Unhandled() => ExecutionExtensions
                .Try(action)
                .Catch<FalseException>(_ => throw TestExtensions.FalseException())
                .Invoke();

            Assert.Throws<TrueException>(Unhandled);
        }

        [Fact]
        internal void SimpleTest()
        {
            Action action = () => { };

            ExecutionExtensions
                .Try(action)
                .Catch<FalseException>()
                .Catch<TrueException>()
                .Invoke();
        }

        [Fact]
        internal void HandledExceptionTest()
        {
            Action action = () => throw FalseException();

            ExecutionExtensions
                .Try(action)
                .Catch<FalseException>()
                .Invoke();
        }

        [Fact]
        internal void SeveralCatchBlocksTest()
        {
            Action action = () => throw FalseException();

            ExecutionExtensions
                .Try(action)
                .Catch<TrueException>(ex => throw ex)
                .Catch<FalseException>()
                .Invoke();
        }

        [Fact]
        internal void ThrowInCatchBlockTest()
        {
            Action action = () => throw FalseException();

            void TestAction() => ExecutionExtensions
                .Try(action)
                .Catch<FalseException>(ex => throw TrueException())
                .Invoke();

            Assert.Throws<TrueException>(TestAction);
        }

        [Fact]
        internal void ThrowInFinallyBlockTest()
        {
            Action action = () => throw FalseException();

            void TestAction() => ExecutionExtensions
                .Try(action)
                .Catch<FalseException>(ex => throw ex)
                .Finally(() => throw TrueException())
                .Invoke();

            Assert.Throws<TrueException>(TestAction);
        }

        private static FalseException FalseException()
        {
            return new FalseException(nameof(FalseException), null);
        }

        private static TrueException TrueException()
        {
            return new TrueException(nameof(TrueException), null);
        }
    }
}