namespace SpaceEngineers.Core.PathResolver
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    /// <summary>
    /// Service for resolve shortest path on the graph between two nodes
    /// </summary>
    /// <typeparam name="TKey">Node key type</typeparam>
    /// <typeparam name="TValue">Node value type</typeparam>
    public interface IPathResolver<TKey, TValue>
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

        /// <summary>
        /// Get all paths from RootNode on graph
        /// </summary>
        /// <param name="genericGraph"> GenericGraph </param>
        /// <param name="rootNodeKey"> Path start node </param>
        /// <returns> All paths from RootNode </returns>
        IEnumerable<Queue<KeyValuePair<TKey, ICollection<TValue>>>> GetAllGroupedPaths(
            GenericGraph<TKey, TValue> genericGraph,
            TKey rootNodeKey);

        /// <summary>
        /// Get all weighted paths from RootNode on graph
        /// </summary>
        /// <param name="genericGraph"> GenericGraph </param>
        /// <param name="rootNodeKey"> Path start node </param>
        /// <param name="weightFunc"> Weight function </param>
        /// <returns> All weighted paths from RootNode </returns>
        IEnumerable<KeyValuePair<int, Queue<KeyValuePair<TKey, ICollection<TValue>>>>> GetAllGroupedWeightedPaths(
            GenericGraph<TKey, TValue> genericGraph,
            TKey rootNodeKey,
            Func<TValue, int> weightFunc);
    }
}