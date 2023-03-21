namespace SpaceEngineers.Core.Basics.Test
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Enumerations;
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
            var unitOfWork = new TestAsyncUnitOfWork(EnUnitOfWorkBehavior.Regular);
            ExecuteInTransaction(unitOfWork, true, EmptyProducer);

            Assert.True(unitOfWork.Started);
            Assert.True(unitOfWork.Committed);
            Assert.False(unitOfWork.RolledBack);
            Assert.False(unitOfWork.RolledBackByException);
        }

        [Fact]
        internal void RollbackTest()
        {
            var unitOfWork = new TestAsyncUnitOfWork(EnUnitOfWorkBehavior.Regular);
            ExecuteInTransaction(unitOfWork, false, EmptyProducer);

            Assert.True(unitOfWork.Started);
            Assert.False(unitOfWork.Committed);
            Assert.True(unitOfWork.RolledBack);
            Assert.False(unitOfWork.RolledBackByException);
        }

        [Fact]
        internal void RollbackByExceptionTest()
        {
            var unitOfWork = new TestAsyncUnitOfWork(EnUnitOfWorkBehavior.Regular);
            Assert.Throws<TrueException>(() => ExecuteInTransaction(unitOfWork, true, ErrorProducer));

            Assert.True(unitOfWork.Started);
            Assert.False(unitOfWork.Committed);
            Assert.True(unitOfWork.RolledBack);
            Assert.True(unitOfWork.RolledBackByException);
        }

        [Fact]
        internal void NestedStartsTest()
        {
            var unitOfWork = new TestAsyncUnitOfWork(EnUnitOfWorkBehavior.Regular);
            Assert.Throws<InvalidOperationException>(() => ExecuteInTransaction(unitOfWork, true, NestedProducer));

            Assert.True(unitOfWork.Started);
            Assert.False(unitOfWork.Committed);
            Assert.True(unitOfWork.RolledBack);
            Assert.True(unitOfWork.RolledBackByException);

            Task NestedProducer(object context, CancellationToken token)
            {
                ExecuteInTransaction(unitOfWork, true, EmptyProducer);
                return Task.CompletedTask;
            }
        }

        [Fact]
        internal void DoNotRunBehaviorTest()
        {
            var unitOfWork = new TestAsyncUnitOfWork(EnUnitOfWorkBehavior.DoNotRun);
            ExecuteInTransaction(unitOfWork, true, EmptyProducer);

            Assert.False(unitOfWork.Started);
            Assert.False(unitOfWork.Committed);
            Assert.False(unitOfWork.RolledBack);
            Assert.False(unitOfWork.RolledBackByException);
        }

        [Fact]
        internal void SkipProducerBehaviorCommitTest()
        {
            var unitOfWork = new TestAsyncUnitOfWork(EnUnitOfWorkBehavior.SkipProducer);

            var producerExecuted = false;

            ExecuteInTransaction(unitOfWork, true, TrackableProducer);

            Assert.True(unitOfWork.Started);
            Assert.True(unitOfWork.Committed);
            Assert.False(unitOfWork.RolledBack);
            Assert.False(unitOfWork.RolledBackByException);
            Assert.False(producerExecuted);

            Task TrackableProducer(object state, CancellationToken token)
            {
                producerExecuted = true;
                return Task.CompletedTask;
            }
        }

        [Fact]
        internal void SkipProducerBehaviorRollbackTest()
        {
            var unitOfWork = new TestAsyncUnitOfWork(EnUnitOfWorkBehavior.SkipProducer);

            var producerExecuted = false;

            ExecuteInTransaction(unitOfWork, false, TrackableProducer);

            Assert.True(unitOfWork.Started);
            Assert.False(unitOfWork.Committed);
            Assert.True(unitOfWork.RolledBack);
            Assert.False(unitOfWork.RolledBackByException);
            Assert.False(producerExecuted);

            Task TrackableProducer(object state, CancellationToken token)
            {
                producerExecuted = true;
                return Task.CompletedTask;
            }
        }

        private static void ExecuteInTransaction(
            IAsyncUnitOfWork<object> unitOfWork,
            bool saveChanges,
            Func<object, CancellationToken, Task> producer)
        {
            ExecutionExtensions
                .Try(ExecuteInTransaction, (unitOfWork, saveChanges, producer))
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
            private readonly EnUnitOfWorkBehavior _behavior;

            public TestAsyncUnitOfWork(EnUnitOfWorkBehavior behavior)
            {
                _behavior = behavior;
            }

            internal bool Started { get; private set; }

            internal bool Committed { get; private set; }

            internal bool RolledBack { get; private set; }

            internal bool RolledBackByException { get; private set; }

            protected override Task<EnUnitOfWorkBehavior> Start(object context, CancellationToken token)
            {
                Started = _behavior != EnUnitOfWorkBehavior.DoNotRun;
                return Task.FromResult(_behavior);
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