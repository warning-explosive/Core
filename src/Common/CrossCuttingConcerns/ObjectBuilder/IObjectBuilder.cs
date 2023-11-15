namespace SpaceEngineers.Core.CrossCuttingConcerns.ObjectBuilder
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// IObjectBuilder
    /// </summary>
    public interface IObjectBuilder
    {
        /// <summary>
        /// Builds object from values
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="values">Property values</param>
        /// <returns>Created and filled instance</returns>
        object? Build(Type type, IDictionary<string, object?>? values = null);

        /// <summary>
        /// Fills object with values
        /// </summary>
        /// <param name="type">Object type</param>
        /// <param name="instance">Object instance</param>
        /// <param name="values">values</param>
        void Fill(Type type, object instance, IDictionary<string, object?> values);
    }

    /// <summary>
    /// IObjectBuilder
    /// </summary>
    /// <typeparam name="T">T type-argument</typeparam>
    public interface IObjectBuilder<out T>
    {
        /// <summary>
        /// Builds object from values
        /// </summary>
        /// <param name="values">Property values</param>
        /// <returns>Created and filled instance</returns>
        T? Build(IDictionary<string, object?>? values = null);
    }
}