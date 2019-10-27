namespace SpaceEngineers.Core.CompositionRoot.Abstractions
{
    using System;

    /// <summary>
    /// Represents decorator for service which register by condition
    /// </summary>
    public interface IConditionalDecorator<TResolvable, TAttribute>
        where TResolvable : IResolvable
        where TAttribute : Attribute
    {
        TResolvable Decoratee { get; }
    }
}