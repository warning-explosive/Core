namespace SpaceEngineers.Core.CompositionRoot.Abstractions
{
    /// <summary>
    /// Represents decorator for service collection
    /// </summary>
    public interface ICollectionDecorator<TCollectionResolvable>
        where TCollectionResolvable : ICollectionResolvable
    {
        TCollectionResolvable Decoratee { get; }
    }
}