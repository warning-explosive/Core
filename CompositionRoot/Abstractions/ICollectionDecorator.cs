namespace SpaceEngineers.Core.CompositionRoot.Abstractions
{
    /// <summary>
    /// Represents decorator for service collection
    /// </summary>
    /// <typeparam name="TCollectionResolvable">ICollectionResolvable</typeparam>
    public interface ICollectionDecorator<TCollectionResolvable>
        where TCollectionResolvable : ICollectionResolvable
    {
        /// <summary>
        /// Decoratee
        /// </summary>
        TCollectionResolvable Decoratee { get; }
    }
}