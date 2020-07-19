namespace SpaceEngineers.Core.AutoWiringApi.Abstractions
{
    /// <summary>
    /// Represents decorator for service collection
    /// </summary>
    /// <typeparam name="TCollectionResolvable">ICollectionResolvable</typeparam>
    public interface ICollectionDecorator<TCollectionResolvable>
        where TCollectionResolvable : class
    {
        /// <summary>
        /// Decoratee
        /// </summary>
        TCollectionResolvable Decoratee { get; }
    }
}