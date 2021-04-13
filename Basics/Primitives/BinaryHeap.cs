namespace SpaceEngineers.Core.Basics.Primitives
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Enumerations;

    /// <summary>
    /// Binary-heap
    /// </summary>
    /// <typeparam name="TElement">TElement type-argument</typeparam>
    public class BinaryHeap<TElement> : IHeap<TElement>
        where TElement : IComparable<TElement>
    {
        private const int Root = 0;

        private readonly EnOrderingKind _orderingKind;
        private readonly object _sync;

        private int _last;
        private int _height;
        private TElement[] _array;

        /// <summary> .cctor </summary>
        /// <param name="orderingKind">EnOrderingKind</param>
        public BinaryHeap(EnOrderingKind orderingKind)
        {
            _orderingKind = orderingKind;
            _sync = new object();

            _last = -1;
            _height = 0;
            _array = Array.Empty<TElement>();
        }

        /// <summary> .cctor </summary>
        /// <param name="source">Source stream</param>
        /// <param name="orderingKind">EnOrderingKind</param>
        public BinaryHeap(IEnumerable<TElement> source, EnOrderingKind orderingKind)
            : this(orderingKind)
        {
            foreach (var element in source)
            {
                Insert(element);
            }
        }

        /// <summary>
        /// Changed event
        /// </summary>
        public event EventHandler? Changed;

        /// <inheritdoc />
        public int Count
        {
            get
            {
                lock (_sync)
                {
                    return _last + 1;
                }
            }
        }

        /// <inheritdoc />
        public bool IsEmpty
        {
            get
            {
                lock (_sync)
                {
                    return _last < 0;
                }
            }
        }

        /// <inheritdoc />
        public IEnumerator<TElement> GetEnumerator()
        {
            return new BinaryHeapEnumerator<TElement>(this);
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <inheritdoc />
        public override string ToString()
        {
            lock (_sync)
            {
                return _array
                    .Select((element, i) =>
                    {
                        var height = Height(i);
                        return (element, height);
                    })
                    .GroupBy(pair => pair.height)
                    .Select(pair => pair.Select(it => it.element))
                    .Select(pair => pair.ToString(" "))
                    .ToString(Environment.NewLine);
            }
        }

        /// <inheritdoc />
        public TElement[] ExtractArray()
        {
            lock (_sync)
            {
                var result = new TElement[_last + 1];

                for (var i = 0; i < result.Length; i++)
                {
                    result[i] = Extract();
                }

                return result;
            }
        }

        /// <inheritdoc />
        public void Insert(TElement element)
        {
            /*
             * 1. Add a new element to the end of the heap
             * 2. Heapify-up starting from the last element
             */

            lock (_sync)
            {
                AddLast(element);
                HeapifyUp(_last);
                NotifyChanged();
            }
        }

        /// <inheritdoc />
        public TElement Extract()
        {
            /*
             * 1. Replace the root of the heap with the last element
             * 2. Heapify-down starting from the root element
             */

            lock (_sync)
            {
                if (_last < 0)
                {
                    throw new InvalidOperationException($"{nameof(BinaryHeap<TElement>)} is empty");
                }

                var result = _array[Root];
                Swap(Root, _last);
                RemoveLast();
                HeapifyDown(Root);

                NotifyChanged();

                return result;
            }
        }

        private void HeapifyUp(int child)
        {
            /*
             * 1. Compare the added element with its parent
             *    If they are in the correct order, stop
             *    If not, swap the element with its parent and return to the previous step
             */

            var parent = Parent(child);

            if (HasHighestPriority(child, parent))
            {
                Swap(child, parent);
                HeapifyUp(parent);
            }
        }

        private void HeapifyDown(int parent)
        {
            /*
             * 1. Compare a parent with his children
             *    If they are in the correct order, stop
             * 2. If not, swap the parent with one of his children and return to the previous step
             *    Swap with the highest priority child
             */

            var theHighestPriority = parent;
            var left = Left(theHighestPriority);
            var right = Right(theHighestPriority);

            if (left <= _last
                && HasHighestPriority(left, theHighestPriority))
            {
                theHighestPriority = left;
            }

            if (right <= _last
                && HasHighestPriority(right, theHighestPriority))
            {
                theHighestPriority = right;
            }

            if (theHighestPriority == parent)
            {
                return;
            }

            Swap(parent, theHighestPriority);
            HeapifyDown(theHighestPriority);
        }

        private bool HasHighestPriority(int index, int theHighestPriority)
        {
            var result = _array[index].CompareTo(_array[theHighestPriority]);

            return _orderingKind == EnOrderingKind.Asc
                ? result < 0
                : result > 0;
        }

        private void AddLast(TElement element)
        {
            Expand();

            _last++;
            _array[_last] = element;
        }

        private void RemoveLast()
        {
            _array[_last] = default(TElement) !;
            _last--;

            Contract();
        }

        private void Expand()
        {
            if (_last + 1 < _array.Length)
            {
                return;
            }

            _array = Resize(_array, ++_height, _last + 1);
        }

        private void Contract()
        {
            var limitedCapacity = Capacity(_height - 1);

            if (limitedCapacity > _last
                && limitedCapacity < _array.Length)
            {
                _array = Resize(_array, --_height, _last + 1);
            }
        }

        private static TElement[] Resize(TElement[] source, int height, int length)
        {
            var resized = new TElement[Capacity(height)];
            Array.Copy(source, resized, length);
            return resized;
        }

        private void Swap(int left, int right)
        {
            var tmp = _array[left];
            _array[left] = _array[right];
            _array[right] = tmp;
        }

        private static int Left(int index) => (2 * index) + 1;

        private static int Right(int index) => (2 * index) + 2;

        private static int Parent(int index) => (index - 1) / 2;

        private static uint Height(int index) => (index + 1).Log(2);

        private static int Capacity(int height) => 2.Pow((uint)height) - 1;

        private void NotifyChanged()
        {
            Changed?.Invoke(this, new EventArgs());
        }

        private class BinaryHeapEnumerator<T> : IEnumerator<T>
            where T : IComparable<T>
        {
            private readonly BinaryHeap<T> _heap;

            private int _current;
            private bool _changed;

            public BinaryHeapEnumerator(BinaryHeap<T> heap)
            {
                _heap = heap;
                _current = -1;

                _heap.Changed += OnChanged;
            }

            public T Current
            {
                get
                {
                    ValidateNoChanges();
                    return _heap._array[_current];
                }
            }

            object IEnumerator.Current => Current;

            public bool MoveNext()
            {
                ValidateNoChanges();
                return ++_current <= _heap._last;
            }

            public void Reset()
            {
                _current = -1;
            }

            public void Dispose()
            {
                _heap.Changed -= OnChanged;
                Reset();
            }

            private void OnChanged(object sender, EventArgs e)
            {
                _changed = true;
            }

            private void ValidateNoChanges()
            {
                if (_changed)
                {
                    throw new InvalidOperationException("Collection was modified during enumeration.");
                }
            }
        }
    }
}