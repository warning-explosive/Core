namespace SpaceEngineers.Core.Basics.Test
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// AsyncUnitOfWorkTest
    /// </summary>
    public class AsyncUnitOfWorkTest : BasicsTestBase
    {
        /// <summary> .ctor </summary>
        /// <param name="output">ITestOutputHelper</param>
        public AsyncUnitOfWorkTest(ITestOutputHelper output)
            : base(output)
        {
        }

        [Fact]
        internal void SeveralStartsTest()
        {
            var unitOfWork = new TestAsyncUnitOfWork();
            Assert.Throws<InvalidOperationException>(() => UnwrapError(() => Run(unitOfWork).Wait()));

            Assert.True(unitOfWork.Started);
            Assert.False(unitOfWork.Committed);
            Assert.True(unitOfWork.RolledBack);

            async Task Run(IAsyncUnitOfWork<object> uow)
            {
                await using (StartTransaction(uow))
                {
                    await using (StartTransaction(uow))
                    {
                    }
                }
            }
        }

        [Fact]
        internal async Task AccessBeforeStartTest()
        {
            var unitOfWork = new TestAsyncUnitOfWork();
            Assert.Throws<InvalidOperationException>(() => unitOfWork.Context);
            Assert.Throws<InvalidOperationException>(() => unitOfWork.SaveChanges());

            Assert.False(unitOfWork.Started);
            Assert.False(unitOfWork.Committed);
            Assert.False(unitOfWork.RolledBack);

            await using (StartTransaction(unitOfWork))
            {
                _ = unitOfWork.Context;
                unitOfWork.SaveChanges();
            }

            Assert.True(unitOfWork.Started);
            Assert.True(unitOfWork.Committed);
            Assert.False(unitOfWork.RolledBack);
        }

        [Fact]
        internal async Task CommitTest()
        {
            var unitOfWork = new TestAsyncUnitOfWork();
            await using (StartTransaction(unitOfWork))
            {
                unitOfWork.SaveChanges();
            }

            Assert.True(unitOfWork.Started);
            Assert.True(unitOfWork.Committed);
            Assert.False(unitOfWork.RolledBack);
        }

        [Fact]
        internal async Task RollbackTest()
        {
            var unitOfWork = new TestAsyncUnitOfWork();
            await using (StartTransaction(unitOfWork))
            {
            }

            Assert.True(unitOfWork.Started);
            Assert.False(unitOfWork.Committed);
            Assert.True(unitOfWork.RolledBack);
        }

        [Fact]
        internal void RollbackByExceptionTest()
        {
            var unitOfWork = new TestAsyncUnitOfWork();
            Assert.Throws<InvalidOperationException>(() => UnwrapError(() => Run(unitOfWork).Wait()));

            Assert.True(unitOfWork.Started);
            Assert.False(unitOfWork.Committed);
            Assert.True(unitOfWork.RolledBack);

            static void Throw()
            {
                throw new InvalidOperationException();
            }

            static async Task Run(IAsyncUnitOfWork<object> uow)
            {
                await using (StartTransaction(uow))
                {
                    Throw();

                    uow.SaveChanges();
                }
            }
        }

        private static IAsyncDisposable StartTransaction(IAsyncUnitOfWork<object> unitOfWork)
        {
             return unitOfWork.StartTransaction(new object(), CancellationToken.None).Result;
        }

        private static void UnwrapError(Action action)
        {
            action
                .Try()
                .Catch<AggregateException>(ex => throw ex.Unwrap().First())
                .Invoke();
        }

        private class TestAsyncUnitOfWork : AsyncUnitOfWork<object>
        {
            internal bool Started { get; private set; }

            internal bool Committed { get; private set; }

            internal bool RolledBack { get; private set; }

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

            protected override Task Rollback(object context, CancellationToken token)
            {
                RolledBack = true;
                return Task.CompletedTask;
            }
        }
    }
}