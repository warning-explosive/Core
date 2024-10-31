namespace SpaceEngineers.Core.Basics.Heap;

public class RootNodeChangedEventArgs<TElement>
{
    public RootNodeChangedEventArgs(TElement? originalValue, TElement? currentValue)
    {
        OriginalValue = originalValue;
        CurrentValue = currentValue;
    }

    public TElement? OriginalValue { get; }

    public TElement? CurrentValue { get; }
}