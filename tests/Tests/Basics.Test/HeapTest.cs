namespace SpaceEngineers.Core.Basics.Test;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Heap;
using Xunit;
using Xunit.Abstractions;

public class HeapTest : BasicsTestBase
{
    public HeapTest(ITestOutputHelper output)
        : base(output)
    {
    }

    public static IEnumerable<object[]> HeapTestData()
    {
        var count = 100;
        var array = new int[count];
        var random = new Random(100);

        for (var i = 0; i < count; i++)
        {
            array[i] = random.Next(0, count);
        }

        yield return new object[] { new BinaryHeap<int>(array, EnOrderingDirection.Asc), EnOrderingDirection.Asc };
        yield return new object[] { new BinaryHeap<int>(array, EnOrderingDirection.Desc), EnOrderingDirection.Desc };
    }

    [Theory]
    [MemberData(nameof(HeapTestData))]
    internal void OrderingTest(IHeap<int> heap, EnOrderingDirection orderingKind)
    {
        Output.WriteLine(heap.ToString());

        var enumeratedArray = (orderingKind == EnOrderingDirection.Asc
                ? heap.OrderBy(it => it)
                : heap.OrderByDescending(it => it))
            .ToArray();

        var orderedArray = heap.ExtractArray();

        Assert.Equal(enumeratedArray, orderedArray);
        Assert.True(heap.IsEmpty);
        Assert.Equal(0, heap.Count);
    }

    [Theory]
    [MemberData(nameof(HeapTestData))]
    internal void MultiThreadAccessTest(IHeap<int> heap, EnOrderingDirection orderingKind)
    {
        var count = heap.Count;

        Output.WriteLine(heap.ToString());

        var enumeratedArray = (orderingKind == EnOrderingDirection.Asc
                ? heap.OrderBy(it => it)
                : heap.OrderByDescending(it => it))
            .ToArray();

        Parallel.For(
            0,
            count,
            new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount },
            Modify);

        var orderedArray = heap.ExtractArray();

        Assert.Equal(enumeratedArray, orderedArray);
        Assert.True(heap.IsEmpty);
        Assert.Equal(0, heap.Count);

        void Modify(int index)
        {
            switch (index % 2)
            {
                case 0: Read(); break;
                default: Write(); break;
            }
        }

        void Read()
        {
            lock (heap)
            {
                _ = heap.Count;
                _ = heap.IsEmpty;
                _ = heap.Peek();
                _ = heap.TryPeek(out _);
            }
        }

        void Write()
        {
            lock (heap)
            {
                heap.Insert(heap.Extract());

                if (heap.TryExtract(out var element))
                {
                    heap.Insert(element);
                }
            }
        }
    }
}