namespace SpaceEngineers.Core.Dynamic.Api.Abstractions
{
    using System;
    using System.Collections.Generic;
    using AutoWiring.Api.Abstractions;

    /// <summary>
    /// IDynamicClassProvider
    /// </summary>
    public interface IDynamicClassProvider : IResolvable
    {
        /// <summary>
        /// Creates dynamic class and initializes instance with specified parameters
        /// </summary>
        /// <param name="dynamicClass">Dynamic class info</param>
        /// <param name="values">Instance values</param>
        /// <returns>Initialized instance</returns>
        object CreateInstance(DynamicClass dynamicClass, IReadOnlyDictionary<DynamicProperty, object?> values);

        /// <summary>
        /// Creates dynamic class
        /// </summary>
        /// <param name="dynamicClass">Dynamic class info</param>
        /// <returns>Dynamically created class</returns>
        Type CreateType(DynamicClass dynamicClass);
    }
}