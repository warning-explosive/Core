namespace SpaceEngineers.Core.Dynamic.Api.Abstractions
{
    using System;
    using AutoWiring.Api.Abstractions;

    /// <summary>
    /// IDynamicClassProvider
    /// </summary>
    public interface IDynamicClassProvider : IResolvable
    {
        /// <summary>
        /// Creates dynamic class
        /// </summary>
        /// <param name="dynamicClass">Dynamic class info</param>
        /// <returns>Dynamically created class</returns>
        Type Create(DynamicClass dynamicClass);
    }
}