namespace SpaceEngineers.Core.Basics.Test
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Primitives;
    using Xunit;
    using Xunit.Abstractions;
    using Xunit.Sdk;

    /// <summary>
    /// AsyncUnitOfWorkTest
    /// </summary>
    public class AsyncUnitOfWorkTest : BasicsTestBase
    {
        private static readonly Func<object, CancellationToken, Task> EmptyProducer = (_, _) => Task.CompletedTask;
        private static readonly Func<object, CancellationToken, Task> ErrorProducer = (_, _) => throw new TrueException(nameof(TrueException), null);

        /// <summary> .ctor </summary>
        /// <param name="output">ITestOutputHelper</param>
        public AsyncUnitOfWorkTest(ITestOutputHelper output)
            : base(output)
        {
        }

        [Fact]
        internal void CommitTest()
        {
            var unitOfWork = new TestAsyncUnitOfWork();
            ExecuteInTransaction(unitOfWork, true, EmptyProducer);

            Assert.True(unitOfWork.Started);
            Assert.True(unitOfWork.Committed);
            Assert.False(unitOfWork.RolledBack);
            Assert.False(unitOfWork.RolledBackByException);
        }

        [Fact]
        internal void RollbackTest()
        {
            var unitOfWork = new TestAsyncUnitOfWork();
            ExecuteInTransaction(unitOfWork, false, EmptyProducer);

            Assert.True(unitOfWork.Started);
            Assert.False(unitOfWork.Committed);
            Assert.True(unitOfWork.RolledBack);
            Assert.False(unitOfWork.RolledBackByException);
        }

        [Fact]
        internal void RollbackByExceptionTest()
        {
            var unitOfWork = new TestAsyncUnitOfWork();
            Assert.Throws<TrueException>(() => ExecuteInTransaction(unitOfWork, true, ErrorProducer));

            Assert.True(unitOfWork.Started);
            Assert.False(unitOfWork.Committed);
            Assert.True(unitOfWork.RolledBack);
            Assert.True(unitOfWork.RolledBackByException);
        }

        [Fact]
        internal void SeveralStartsTest()
        {
            var unitOfWork = new TestAsyncUnitOfWork();
            Assert.Throws<InvalidOperationException>(() => ExecuteInTransaction(unitOfWork, true, OuterProducer));

            Assert.True(unitOfWork.Started);
            Assert.False(unitOfWork.Committed);
            Assert.True(unitOfWork.RolledBack);
            Assert.True(unitOfWork.RolledBackByException);

            Task OuterProducer(object context, CancellationToken token)
            {
                ExecuteInTransaction(unitOfWork, true, EmptyProducer);
                return Task.CompletedTask;
            }
        }

        private static void ExecuteInTransaction(
            IAsyncUnitOfWork<object> unitOfWork,
            bool saveChanges,
            Func<object, CancellationToken, Task> producer)
        {
            ExecutionExtensions
                .Try((unitOfWork, saveChanges, producer), ExecuteInTransaction)
                .Catch<AggregateException>(ex => throw ex.RealException())
                .Invoke();
        }

        private static void ExecuteInTransaction(
            (IAsyncUnitOfWork<object>, bool, Func<object, CancellationToken, Task>) state)
        {
            var (unitOfWork, saveChanges, producer) = state;
            unitOfWork.ExecuteInTransaction(new object(), producer, saveChanges, CancellationToken.None).Wait();
        }

        private class TestAsyncUnitOfWork : AsyncUnitOfWork<object>
        {
            internal bool Started { get; private set; }

            internal bool Committed { get; private set; }

            internal bool RolledBack { get; private set; }

            internal bool RolledBackByException { get; private set; }

            protected override Task Start(object context, CancellationToken token)
            {
                Started = true;
                return Task.CompletedTask;
            }

            protected override Task Commit(object context, CancellationToken token)
            {
                Committed = true;
                return Task.CompletedTask;
            }

            protected override Task Rollback(object context, Exception? exception, CancellationToken token)
            {
                RolledBack = true;
                RolledBackByException = exception != null;
                return Task.CompletedTask;
            }
        }
    }
}