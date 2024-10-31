namespace SpaceEngineers.Core.Basics.Heap;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

public interface IHeap<TElement> : IEnumerable<TElement>
    where TElement : IEquatable<TElement>, IComparable<TElement>, IComparable
{
    event EventHandler<RootNodeChangedEventArgs<TElement>>? RootNodeChanged;

    int Count { get; }

    bool IsEmpty { get; }

    void Insert(TElement element);

    TElement Peek();

    bool TryPeek([NotNullWhen(true)] out TElement? element);

    TElement Extract();

    bool TryExtract([NotNullWhen(true)] out TElement? element);

    TElement[] ExtractArray();
}