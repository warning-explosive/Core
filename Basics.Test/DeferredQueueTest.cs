namespace SpaceEngineers.Core.Basics.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Enumerations;
    using Primitives;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// DeferredQueue primitive test
    /// </summary>
    public class DeferredQueueTest : BasicsTestBase
    {
        /// <summary> .cctor </summary>
        /// <param name="output">ITestOutputHelper</param>
        public DeferredQueueTest(ITestOutputHelper output)
            : base(output)
        {
        }

        /// <summary>
        /// OnRootNodeChangedTestData
        /// </summary>
        /// <returns>TestData</returns>
        public static IEnumerable<object> DeferredQueueTestData()
        {
            var emptyQueue = new DeferredQueue<Entry>(new BinaryHeap<HeapEntry<Entry, DateTime>>(EnOrderingKind.Asc), PrioritySelector);

            yield return new object[] { emptyQueue };

            static DateTime PrioritySelector(Entry entry) => entry.Planned;
        }

        [Theory]
        [MemberData(nameof(DeferredQueueTestData))]
        internal async Task OnRootNodeChangedTest(DeferredQueue<Entry> queue)
        {
            Assert.Throws<NotSupportedException>(queue.Dequeue);
            Assert.Throws<NotSupportedException>(() => queue.TryDequeue(out _));
            Assert.Throws<NotSupportedException>(queue.Peek);
            Assert.Throws<NotSupportedException>(() => queue.TryPeek(out _));

            Assert.True(queue.IsEmpty);

            var step = TimeSpan.FromMilliseconds(100);
            var startFrom = DateTime.Now.Add(step);

            queue.Enqueue(new Entry(0, startFrom));
            queue.Enqueue(new Entry(2, startFrom.Add(2 * step)));
            queue.Enqueue(new Entry(4, startFrom.Add(4 * step)));

            var entries = new List<Entry>();
            var started = DateTime.Now;
            Output.WriteLine($"Started at: {started:O}");
            using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3)))
            {
                var backgroundPublisher = Task.Run(async () =>
                    {
                        var corrected = startFrom.Add(step / 2) - DateTime.Now;

                        await Task.Delay(corrected, cts.Token).ConfigureAwait(false);
                        queue.Enqueue(new Entry(1, startFrom.Add(1 * step)));

                        await Task.Delay(step, cts.Token).ConfigureAwait(false);
                        queue.Enqueue(new Entry(3, startFrom.Add(3 * step)));

                        await Task.Delay(step, cts.Token).ConfigureAwait(false);
                        queue.Enqueue(new Entry(5, startFrom.Add(5 * step)));
                    },
                    cts.Token);

                var deferredDeliveryOperation = queue.Run(Callback, cts.Token);

                await Task.WhenAll(backgroundPublisher, deferredDeliveryOperation).ConfigureAwait(false);
            }

            entries.Each(entry => Output.WriteLine(entry.ToString()));
            Assert.True(queue.IsEmpty);
            Assert.True(started <= entries.First().Planned);

            var deltas = new List<TimeSpan>();
            _ = entries.Aggregate((prev, next) =>
            {
                var delta = next.Actual - prev.Actual;
                deltas.Add(delta);
                return next;
            });

            deltas.Each(delta => Output.WriteLine(delta.ToString()));
            Assert.Equal(Enumerable.Range(0, 6).ToArray(), entries.Select(entry => entry.Index).ToArray());

            Task Callback(Entry entry)
            {
                var now = DateTime.Now;

                Assert.True(entry.Planned <= now);

                entries.Add(entry);
                entry.Actual = now;

                return Task.CompletedTask;
            }
        }

        [Theory(Timeout = 60_000)]
        [MemberData(nameof(DeferredQueueTestData))]
        internal async Task IntensiveReadWriteTest(DeferredQueue<Entry> queue)
        {
            Assert.True(queue.IsEmpty);

            var publishersCount = 2;
            var count = 500;
            var actualCount = 0;
            var timeout = TimeSpan.FromSeconds(10);
            var step = TimeSpan.FromMilliseconds(1);
            var startFrom = DateTime.Now.Add(TimeSpan.FromMilliseconds(100));

            var started = DateTime.Now;

            using (var cts = new CancellationTokenSource(timeout))
            {
                var token = cts.Token;

                var firstBackgroundPublisher = Task.Run(() => StartPublishing(queue, count, startFrom, step, token), token);
                var secondBackgroundPublisher = Task.Run(() => StartPublishing(queue, count, startFrom.Add(step / publishersCount), step, token), token);
                var deferredDeliveryOperation = queue.Run(Callback, token);

                await Task.WhenAll(firstBackgroundPublisher, secondBackgroundPublisher).ConfigureAwait(false);
                await Task.Delay(TimeSpan.FromMilliseconds(100), token).ConfigureAwait(false);

                cts.Cancel();

                await deferredDeliveryOperation.ConfigureAwait(false);
            }

            var finished = DateTime.Now;
            var duration = finished - started;
            Output.WriteLine(duration.ToString());

            Assert.True(queue.IsEmpty);
            Assert.Equal(publishersCount * count, actualCount);

            Task Callback(Entry entry)
            {
                Interlocked.Increment(ref actualCount);
                return Task.CompletedTask;
            }
        }

        private static async Task StartPublishing(
            DeferredQueue<Entry> queue,
            int count,
            DateTime startFrom,
            TimeSpan step,
            CancellationToken token)
        {
            for (var i = 1; i <= count; i++)
            {
                await Task.Delay(step, token).ConfigureAwait(false);
                queue.Enqueue(new Entry(i, startFrom.Add(i * step)));
            }
        }

        internal class Entry : IEquatable<Entry>,
                               ISafelyEquatable<Entry>,
                               ISafelyComparable<Entry>,
                               IComparable<Entry>,
                               IComparable
        {
            private DateTime? _actual;

            public Entry(int index, DateTime planned)
            {
                Index = index;
                Planned = planned;
            }

            public int Index { get; }

            public DateTime Planned { get; }

            public DateTime Actual
            {
                get => _actual.EnsureNotNull<DateTime>("Elapsed should be set");
                set => _actual = value;
            }

            public override string ToString()
            {
                return $"[{Index}] - {Planned:O} - {Actual:O}";
            }

            public override int GetHashCode()
            {
                return Index;
            }

            public override bool Equals(object? obj)
            {
                return Equatable.Equals(this, obj);
            }

            public bool Equals(Entry? other)
            {
                return Equatable.Equals(this, other);
            }

            public bool SafeEquals(Entry other)
            {
                return Index == other.Index;
            }

            public int SafeCompareTo(Entry other)
            {
                return Index.CompareTo(other.Index);
            }

            public int CompareTo(Entry? other)
            {
                return Comparable.CompareTo(this, other);
            }

            public int CompareTo(object? obj)
            {
                return Comparable.CompareTo(this, obj);
            }
        }
    }
}