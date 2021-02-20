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

            action.Try().Catch<TrueException>().Invoke();
            action.Try().Catch<TrueException>(ex => { }).Invoke();

            void HandleCaught() => action
                .Try()
                .Catch<TrueException>()
                .Invoke(ex => throw ex);
            HandleCaught();

            void ThrowNotHandled() => action
                .Try()
                .Catch<FalseException>()
                .Invoke(ex => throw ex);
            Assert.Throws<TrueException>(ThrowNotHandled);

            void ThrowNewNotHandled() => action
                .Try()
                .Catch<FalseException>()
                .Invoke(ex => throw TestExtensions.FalseException());
            Assert.Throws<FalseException>(ThrowNewNotHandled);
        }

        [Fact]
        internal void SimpleTest()
        {
            Action action = () => { };

            action.Try()
                  .Catch<FalseException>()
                  .Catch<TrueException>()
                  .Invoke();
        }

        [Fact]
        internal void HandledExceptionTest()
        {
            Action action = () => throw FalseException();

            action.Try()
                  .Catch<FalseException>()
                  .Invoke();
        }

        [Fact]
        internal void SeveralCatchBlocksTest()
        {
            Action action = () => throw FalseException();

            action.Try()
                  .Catch<TrueException>(ex => throw ex)
                  .Catch<FalseException>()
                  .Invoke();
        }

        [Fact]
        internal void ThrowInCatchBlockTest()
        {
            Action action = () => throw FalseException();

            void TestAction() => action.Try()
                                       .Catch<FalseException>(ex => throw TrueException())
                                       .Invoke();

            Assert.Throws<TrueException>(TestAction);
        }

        [Fact]
        internal void ThrowInInvokeBlockTest()
        {
            Action action = () => throw FalseException();

            void TestAction() => action
                .Try()
                .Catch<TrueException>()
                .Invoke(ex => throw TrueException());

            Assert.Throws<TrueException>(TestAction);
        }

        [Fact]
        internal void ThrowInFinallyBlockTest()
        {
            Action action = () => throw FalseException();

            void TestAction() => action.Try()
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