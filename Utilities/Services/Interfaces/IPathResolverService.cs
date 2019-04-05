namespace SpaceEngineers.Core.Utilities.Services.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Implementations;

    /// <summary>
    /// Path resolver
    /// </summary>
    /// <typeparam name="TKey">Node key type</typeparam>
    /// <typeparam name="TValue">Node value type</typeparam>
    public interface IPathResolverService<TKey, TValue>
        where TKey : struct
        where TValue : IEquatable<TValue>
    {
        /// <summary>
        /// Get single and shortest path from RootNodeKey to TargetNodeKey on graph
        /// </summary>
        /// <param name="genericGraph"> GenericGraph </param>
        /// <param name="pathResolverInfo"> GraphSolverInfo </param>
        /// <returns> Shortest path from RootNodeKey to TargetNodeKey </returns>
        /// <exception cref="Exception"> Path not found </exception>
        /// <exception cref="AmbiguousMatchException"> Ambiguous number of paths </exception>
        Queue<KeyValuePair<TKey, TValue>> GetShortestPath(GenericGraph<TKey, TValue> genericGraph,
                                                          PathResolverInfo<TKey, TValue> pathResolverInfo);
    }
}