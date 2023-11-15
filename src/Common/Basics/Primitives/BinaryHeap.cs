namespace SpaceEngineers.Core.Basics.Primitives
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Enumerations;

    /// <summary>
    /// Binary-heap
    /// </summary>
    /// <typeparam name="TElement">TElement type-argument</typeparam>
    public class BinaryHeap<TElement> : IHeap<TElement>
        where TElement : IEquatable<TElement>, IComparable<TElement>, IComparable
    {
        private const int Root = 0;

        private readonly EnOrderingDirection _orderingDirection;

        private int _last;
        private int _height;
        private TElement[] _array;

        /// <summary> .cctor </summary>
        /// <param name="orderingDirection">EnOrderingDirection</param>
        public BinaryHeap(EnOrderingDirection orderingDirection)
        {
            _orderingDirection = orderingDirection;

            _last = -1;
            _height = 0;
            _array = Array.Empty<TElement>();
        }

        /// <summary> .cctor </summary>
        /// <param name="source">Source stream</param>
        /// <param name="orderingDirection">EnOrderingDirection</param>
        public BinaryHeap(IEnumerable<TElement> source, EnOrderingDirection orderingDirection)
            : this(orderingDirection)
        {
            foreach (var element in source)
            {
                Insert(element);
            }
        }

        /// <inheritdoc />
        public event EventHandler<RootNodeChangedEventArgs<TElement>>? RootNodeChanged;

        private event EventHandler? Changed;

        /// <inheritdoc />
        public int Count => Length();

        /// <inheritdoc />
        public bool IsEmpty => IsEmptyHeap();

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

        /// <inheritdoc />
        public TElement[] ExtractArray()
        {
            var result = new TElement[Length()];

            for (var i = 0; i < result.Length; i++)
            {
                result[i] = ExtractFromHeap();
            }

            return result;
        }

        /// <inheritdoc />
        public void Insert(TElement element)
        {
            /*
             * 1. Add a new element to the end of the heap
             * 2. Heapify-up starting from the last element
             */

            var originalRoot = GetRoot();

            AddLast(element);
            HeapifyUp(_last);

            NotifyChanged();
            NotifyRootNodeChanged(originalRoot);
        }

        /// <inheritdoc />
        public TElement Peek()
        {
            return PeekHeap();
        }

        /// <inheritdoc />
        public bool TryPeek([NotNullWhen(true)] out TElement? element)
        {
            return Try(PeekHeap, out element);
        }

        /// <inheritdoc />
        public TElement Extract()
        {
            return ExtractFromHeap();
        }

        /// <inheritdoc />
        public bool TryExtract([NotNullWhen(true)] out TElement? element)
        {
            return Try(ExtractFromHeap, out element);
        }

        #region internals

        private bool Try(Func<TElement> accessor, [NotNullWhen(true)] out TElement? element)
        {
            if (IsEmptyHeap())
            {
                element = default;
                return false;
            }

            element = accessor();
            return true;
        }

        private TElement PeekHeap()
        {
            return GetRequiredRoot();
        }

        private TElement ExtractFromHeap()
        {
            /*
             * 1. Replace the root of the heap with the last element
             * 2. Heapify-down starting from the root element
             */

            var originalRoot = GetRequiredRoot();

            Swap(Root, _last);
            RemoveLast();
            HeapifyDown(Root);

            NotifyChanged();
            NotifyRootNodeChanged(originalRoot);

            return originalRoot;
        }

        private bool IsEmptyHeap()
        {
            return _last < 0;
        }

        private int Length()
        {
            return _last + 1;
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

            return _orderingDirection == EnOrderingDirection.Asc
                ? result < 0
                : result > 0;
        }

        private TElement? GetRoot()
        {
            return !IsEmptyHeap()
                ? _array[Root]
                : default;
        }

        private TElement GetRequiredRoot()
        {
            if (IsEmptyHeap())
            {
                throw new InvalidOperationException($"{nameof(BinaryHeap<TElement>)} is empty");
            }

            return _array[Root];
        }

        private void AddLast(TElement element)
        {
            Expand();

            _last++;
            _array[_last] = element;
        }

        private void RemoveLast()
        {
            _array[_last] = default!;
            _last--;

            Contract();
        }

        private void Expand()
        {
            if (Length() < _array.Length)
            {
                return;
            }

            _array = Resize(_array, ++_height, Length());
        }

        private void Contract()
        {
            var limitedCapacity = Capacity(_height - 1);

            if (limitedCapacity > _last
                && limitedCapacity < _array.Length)
            {
                _array = Resize(_array, --_height, Length());
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
            if (left == right)
            {
                return;
            }

            (_array[left], _array[right]) = (_array[right], _array[left]);
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

        private void NotifyRootNodeChanged(TElement? originalRoot)
        {
            var currentRoot = GetRoot();

            RootNodeChanged?.Invoke(this, new RootNodeChangedEventArgs<TElement>(originalRoot, currentRoot));
        }

        #endregion

        private class BinaryHeapEnumerator<T> : IEnumerator<T>
            where T : IEquatable<T>, IComparable<T>, IComparable
        {
            private readonly BinaryHeap<T> _heap;
            private readonly EventHandler _subscription;

            private int _current;
            private bool _changed;

            public BinaryHeapEnumerator(BinaryHeap<T> heap)
            {
                _heap = heap;
                _current = -1;

                _subscription = OnChanged;
                _heap.Changed += _subscription;
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
                _heap.Changed -= _subscription;
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
                    throw new InvalidOperationException("Collection was modified during enumeration");
                }
            }
        }
    }
}