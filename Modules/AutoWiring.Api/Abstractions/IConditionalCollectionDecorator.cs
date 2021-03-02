namespace SpaceEngineers.Core.AutoWiring.Api.Abstractions
{
    using System;

    /// <summary>
    /// Represents decorator for service collection which register by condition
    /// </summary>
    /// <typeparam name="TCollectionResolvable">ICollectionResolvable</typeparam>
    /// <typeparam name="TAttribute">Attribute</typeparam>
    public interface IConditionalCollectionDecorator<TCollectionResolvable, TAttribute> : ICollectionDecorator<TCollectionResolvable>
        where TCollectionResolvable : class
        where TAttribute : Attribute
    {
    }
}