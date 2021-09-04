namespace SpaceEngineers.Core.Basics.Test
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Primitives;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// Test for async synchronization primitives
    /// </summary>
    public class AsyncSynchronizationPrimitivesTest : BasicsTestBase
    {
        private static readonly TimeSpan TestTimeout = TimeSpan.FromSeconds(1);

        /// <summary> .cctor </summary>
        /// <param name="output">ITestOutputHelper</param>
        public AsyncSynchronizationPrimitivesTest(ITestOutputHelper output)
            : base(output)
        {
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        internal async Task AsyncManualResetEventTest(bool isSet)
        {
            var manualResetEvent = new AsyncManualResetEvent(isSet);

            var waitTask1 = manualResetEvent.WaitAsync();
            var waitTask2 = manualResetEvent.WaitAsync();
            var waitTask3 = manualResetEvent.WaitAsync();

            var waitAll = Task.WhenAll(waitTask1, waitTask2, waitTask3);

            Assert.True(isSet ? waitAll.IsCompleted : !waitAll.IsCompleted);

            if (!isSet)
            {
                manualResetEvent.Set();
            }

            var actual = await Task.WhenAny(waitAll, Task.Delay(TestTimeout)).ConfigureAwait(false);

            Assert.Equal(waitAll, actual);

            Assert.True(manualResetEvent.WaitAsync().IsCompleted);

            manualResetEvent.Reset();

            var waitTaskAfterReset1 = manualResetEvent.WaitAsync();
            var waitTaskAfterReset2 = manualResetEvent.WaitAsync();
            var waitTaskAfterReset3 = manualResetEvent.WaitAsync();

            Assert.False(waitTaskAfterReset1.IsCompleted);
            Assert.False(waitTaskAfterReset2.IsCompleted);
            Assert.False(waitTaskAfterReset3.IsCompleted);

            var waitAllAfterReset = Task.WhenAll(waitTask1, waitTask2, waitTask3);

            manualResetEvent.Set();

            var actualAfterReset = await Task.WhenAny(waitAllAfterReset, Task.Delay(TestTimeout)).ConfigureAwait(false);

            Assert.Equal(waitAllAfterReset, actualAfterReset);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        internal async Task AsyncAutoResetEventTest(bool isSet)
        {
            var autoResetEvent = new AsyncAutoResetEvent(isSet);

            var waitTask1 = autoResetEvent.WaitAsync();
            var waitTask2 = autoResetEvent.WaitAsync();
            var waitTask3 = autoResetEvent.WaitAsync();

            Assert.True(isSet ? waitTask1.IsCompleted : !waitTask1.IsCompleted);

            var waitAll = Task.WhenAll(waitTask1, waitTask2, waitTask3);
            var timeout = Task.Delay(TestTimeout);
            var actual = await Task.WhenAny(waitAll, timeout).ConfigureAwait(false);

            Assert.Equal(timeout, actual);

            var range = isSet ? 2 : 3;
            Enumerable.Range(0, range).Each(_ => autoResetEvent.Set());

            timeout = Task.Delay(TestTimeout);
            actual = await Task.WhenAny(waitAll, timeout).ConfigureAwait(false);

            Assert.Equal(waitAll, actual);
            Assert.False(autoResetEvent.WaitAsync().IsCompleted);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(3)]
        internal async Task AsyncCountdownEventTest(int initialCount)
        {
            var countdownEvent = new AsyncCountdownEvent(initialCount);

            Assert.True(initialCount <= 0
                ? countdownEvent.WaitAsync().IsCompleted
                : !countdownEvent.WaitAsync().IsCompleted);

            if (initialCount <= 0)
            {
                Enumerable
                    .Range(0, 3)
                    .Each(_ => countdownEvent.Increment());
            }

            Assert.Equal(3, countdownEvent.Read());
            Assert.False(countdownEvent.WaitAsync().IsCompleted);

            Enumerable
                .Range(0, 3 - 1)
                .Each(_ =>
                {
                    countdownEvent.Decrement();
                    Assert.False(countdownEvent.WaitAsync().IsCompleted);
                });

            Assert.Equal(0, countdownEvent.Decrement());
            Assert.True(countdownEvent.WaitAsync().IsCompleted);
            await countdownEvent.WaitAsync().ConfigureAwait(false);
        }
    }
}