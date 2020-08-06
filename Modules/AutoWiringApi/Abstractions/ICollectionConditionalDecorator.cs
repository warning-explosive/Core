namespace SpaceEngineers.Core.AutoWiringApi.Abstractions
{
    using System;

    /// <summary>
    /// Represents decorator for service collection which register by condition
    /// </summary>
    /// <typeparam name="TCollectionResolvable">ICollectionResolvable</typeparam>
    /// <typeparam name="TAttribute">Attribute</typeparam>
    public interface ICollectionConditionalDecorator<TCollectionResolvable, TAttribute>
        where TCollectionResolvable : class
        where TAttribute : Attribute
    {
        /// <summary>
        /// Decoratee
        /// </summary>
        TCollectionResolvable Decoratee { get; }
    }
}