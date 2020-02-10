namespace SpaceEngineers.Core.CompositionInfoExtractor
{
    using System;
    using AutoWiringApi.Abstractions;

    /// <summary>
    /// Close open or partially-closed generic types
    /// </summary>
    internal interface IGenericArgumentsReceiver : IResolvable
    {
        /// <summary>
        /// Close open or partially-closed generic type by type constraints
        /// </summary>
        /// <param name="type">Generic type for closing</param>
        /// <returns>Closed generic type</returns>
        Type CloseByConstraints(Type type);
    }
}