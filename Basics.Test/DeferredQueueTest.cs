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
        public static IEnumerable<object> OnRootNodeChangedTestData()
        {
            var emptyQueue = new DeferredQueue<Entry>(new BinaryHeap<HeapEntry<Entry, DateTime>>(EnOrderingKind.Asc), PrioritySelector);

            yield return new object[] { emptyQueue };

            static DateTime PrioritySelector(Entry entry) => entry.Planned;
        }

        [Theory]
        [MemberData(nameof(OnRootNodeChangedTestData))]
        internal async Task OnRootNodeChangedTest(DeferredQueue<Entry> queue)
        {
            Assert.Throws<NotSupportedException>(queue.Dequeue);
            Assert.Throws<NotSupportedException>(() => queue.TryDequeue(out _));
            Assert.Throws<NotSupportedException>(queue.Peek);
            Assert.Throws<NotSupportedException>(() => queue.TryPeek(out _));

            Assert.True(queue.IsEmpty);
            var shifted = DateTime.Now.AddMilliseconds(300);
            queue.Enqueue(new Entry(0, shifted.AddMilliseconds(100)));
            queue.Enqueue(new Entry(1, shifted.AddMilliseconds(300)));
            queue.Enqueue(new Entry(3, shifted.AddMilliseconds(700)));

            var entries = new List<Entry>();
            var started = DateTime.Now;
            Output.WriteLine($"Started at: {started:O}");
            using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3)))
            {
                var backgroundPublisher = Task.Run(async () =>
                    {
                        var delay = shifted.AddMilliseconds(400) - DateTime.Now;
                        Output.WriteLine($"Background delay: {delay}");
                        await Task.Delay(delay, cts.Token).ConfigureAwait(false);
                        Assert.True(DateTime.Now >= shifted.AddMilliseconds(400));
                        Assert.True(DateTime.Now <= shifted.AddMilliseconds(600));

                        queue.Enqueue(new Entry(2, shifted.AddMilliseconds(500)));
                        queue.Enqueue(new Entry(4, shifted.AddMilliseconds(900)));
                        queue.Enqueue(new Entry(5, shifted.AddMilliseconds(1100)));
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