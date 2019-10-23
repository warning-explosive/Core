namespace SpaceEngineers.Core.Utilities.PathResolver
{
    using System;

    /// <summary>
    /// GenericGraph edge
    /// </summary>
    /// <typeparam name="TKey">Node key type</typeparam>
    /// <typeparam name="TValue">Node value type</typeparam>
    public class GenericGraphEdge<TKey, TValue>
        where TKey : struct
        where TValue : IEquatable<TValue>
    {
        /// <summary> .ctor </summary>
        /// <param name="sourceNodeKey">OuterKey => source node key</param>
        /// <param name="targetNodeKey">InnerKey => target node key</param>
        /// <param name="edgeInfo">Value => weight and other</param>
        public GenericGraphEdge(TKey sourceNodeKey, TKey targetNodeKey, TValue edgeInfo)
        {
            SourceNodeKey = sourceNodeKey;
            TargetNodeKey = targetNodeKey;
            EdgeInfo = edgeInfo;
        }

        /// <summary>
        /// OuterKey - source node key
        /// </summary>
        public TKey SourceNodeKey { get; }

        /// <summary>
        /// InnerKey - target node key
        /// </summary>
        public TKey TargetNodeKey { get; }
            
        /// <summary>
        /// Value - weight and other
        /// </summary>
        public TValue EdgeInfo { get; }
    }
}