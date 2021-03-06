namespace SpaceEngineers.Core.PathResolver
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// GraphSolver inputs
    /// </summary>
    /// <typeparam name="TKey">Node key type</typeparam>
    /// <typeparam name="TValue">Node value type</typeparam>
    public class PathResolverInfo<TKey, TValue>
        where TKey : struct
        where TValue : IEquatable<TValue>
    {
        /// <summary> .ctor </summary>
        /// <param name="rootNodeKey"> Root node key </param>
        /// <param name="targetNodeKey"> Target node key </param>
        /// <param name="weightFunc"> Weight func of edge </param>
        public PathResolverInfo(TKey rootNodeKey,
                               TKey targetNodeKey,
                               Func<TValue, int> weightFunc)
        {
            RootNodeKey = rootNodeKey;
            TargetNodeKey = targetNodeKey;
            WeightFunc = weightFunc;
        }

        /// <summary>
        /// Root node key => Tree root node
        /// </summary>
        public TKey RootNodeKey { get; }

        /// <summary>
        /// Target node key => Target leaf key
        /// </summary>
        public TKey TargetNodeKey { get; }

        /// <summary>
        /// Weight func of edge
        /// </summary>
        public Func<TValue, int> WeightFunc { get; }

        /// <summary>
        /// Required node keys (in queue order)
        /// </summary>
        public Queue<TKey> RequiredKeys { get; } = new Queue<TKey>();

        /// <summary>
        /// Required edges (in queue order)
        /// </summary>
        public Queue<TValue> RequiredEdges { get; } = new Queue<TValue>();

        /// <summary>
        /// true => search paths without loops
        /// </summary>
        public bool WithoutLoops { get; set; }

        /// <summary>
        /// If RootNodeKey equals TargetNodeKey search algorithm find full (FULL == NOT EMPTY) circle path
        /// </summary>
        public bool NotEmptyCircle { get; set; }
    }
}