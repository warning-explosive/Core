namespace SpaceEngineers.Core.CrossCuttingConcerns.Api.Abstractions
{
    using System.Collections.Generic;
    using AutoWiring.Api.Abstractions;

    /// <summary>
    /// IObjectCreator
    /// </summary>
    /// <typeparam name="T">T type-argument</typeparam>
    public interface IObjectBuilder<out T> : IResolvable
        where T : class
    {
        /// <summary>
        /// Builds object from values
        /// </summary>
        /// <param name="values">Property values</param>
        /// <returns>Created and filled instance</returns>
        T Build(IDictionary<string, object>? values = null);
    }
}