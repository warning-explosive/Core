namespace SpaceEngineers.Core.CompositionRoot.Abstractions
{
    using System;

    /// <summary>
    /// Represents decorator for service collection which register by condition
    /// </summary>
    public interface ICollectionConditionalDecorator<TCollectionResolvable, TAttribute>
        where TCollectionResolvable : ICollectionResolvable
        where TAttribute : Attribute
    {
        /// <summary>
        /// Decoratee
        /// </summary>
        TCollectionResolvable Decoratee { get; }
    }
}