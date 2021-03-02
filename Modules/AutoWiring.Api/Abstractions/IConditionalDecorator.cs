namespace SpaceEngineers.Core.AutoWiring.Api.Abstractions
{
    using System;

    /// <summary>
    /// Represents decorator for service which register by condition
    /// </summary>
    /// <typeparam name="TResolvable">IResolvable</typeparam>
    /// <typeparam name="TAttribute">Attribute</typeparam>
    public interface IConditionalDecorator<TResolvable, TAttribute> : IDecorator<TResolvable>
        where TResolvable : class
        where TAttribute : Attribute
    {
    }
}