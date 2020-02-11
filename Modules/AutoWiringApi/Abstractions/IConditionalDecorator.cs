namespace SpaceEngineers.Core.AutoWiringApi.Abstractions
{
    using System;

    /// <summary>
    /// Represents decorator for service which register by condition
    /// </summary>
    /// <typeparam name="TResolvable">IResolvable</typeparam>
    /// <typeparam name="TAttribute">Attribute</typeparam>
    public interface IConditionalDecorator<TResolvable, TAttribute>
        where TResolvable : IResolvable
        where TAttribute : Attribute
    {
        /// <summary>
        /// Decoratee
        /// </summary>
        TResolvable Decoratee { get; }
    }
}