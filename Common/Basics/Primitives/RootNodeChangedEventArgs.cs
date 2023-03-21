namespace SpaceEngineers.Core.Basics.Primitives
{
    /// <summary>
    /// RootNodeChangedEventArgs
    /// </summary>
    /// <typeparam name="TElement">TElement type-argument</typeparam>
    public class RootNodeChangedEventArgs<TElement>
    {
        /// <summary> .cctor </summary>
        /// <param name="originalValue">Original value</param>
        /// <param name="currentValue">Current value</param>
        public RootNodeChangedEventArgs(TElement? originalValue, TElement? currentValue)
        {
            OriginalValue = originalValue;
            CurrentValue = currentValue;
        }

        /// <summary>
        /// Original value
        /// </summary>
        public TElement? OriginalValue { get; }

        /// <summary>
        /// Current value
        /// </summary>
        public TElement? CurrentValue { get; }
    }
}