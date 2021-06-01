namespace SpaceEngineers.Core.CrossCuttingConcerns.Api.Abstractions
{
    using System;
    using System.Collections.Generic;
    using AutoWiring.Api.Abstractions;

    /// <summary>
    /// IObjectBuilder
    /// </summary>
    public interface IObjectBuilder : IResolvable
    {
        /// <summary>
        /// Builds object from values
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="values">Property values</param>
        /// <returns>Created and filled instance</returns>
        object? Build(Type type, IDictionary<string, object>? values = null);
    }

    /// <summary>
    /// IObjectBuilder
    /// </summary>
    /// <typeparam name="T">T type-argument</typeparam>
    public interface IObjectBuilder<out T> : IResolvable
    {
        /// <summary>
        /// Builds object from values
        /// </summary>
        /// <param name="values">Property values</param>
        /// <returns>Created and filled instance</returns>
        T? Build(IDictionary<string, object>? values = null);
    }
}