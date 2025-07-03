namespace SpaceEngineers.Core.Basics.Heap;

public class RootNodeChangedEventArgs<TElement>(TElement? originalValue, TElement? currentValue)
{
    public TElement? OriginalValue { get; } = originalValue;

    public TElement? CurrentValue { get; } = currentValue;
}