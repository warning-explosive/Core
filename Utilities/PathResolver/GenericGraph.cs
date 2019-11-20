namespace SpaceEngineers.Core.PathResolver
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Generic graph
    /// </summary>
    /// <typeparam name="TKey">Node key type</typeparam>
    /// <typeparam name="TValue">Node value type</typeparam>
    public class GenericGraph<TKey, TValue>
        where TKey : struct
        where TValue : IEquatable<TValue>
    {
        /// <summary> .ctor </summary>
        /// <param name="inputs">input edge collection</param>
        public GenericGraph(ICollection<GenericGraphEdge<TKey, TValue>> inputs)
        {
            Graph = new Dictionary<TKey, IDictionary<TKey, ICollection<TValue>>>();
            
            foreach (var input in inputs)
            {
                if (!Graph.TryGetValue(input.SourceNodeKey, out var nodeGroup))
                {
                    nodeGroup = new Dictionary<TKey, ICollection<TValue>>();
                    Graph[input.SourceNodeKey] = nodeGroup;
                }

                if (!nodeGroup.TryGetValue(input.TargetNodeKey, out _))
                {
                    nodeGroup[input.TargetNodeKey] = new List<TValue>();
                }
                
                nodeGroup[input.TargetNodeKey].Add(input.EdgeInfo);
            }
        }

        /// <summary>
        /// Graph - collect graph edges
        /// [sourceNode] ==weight=> [targetNode]
        /// OuterKey - source node key
        /// InnerKey - target node key
        /// Value - weight and other
        /// </summary>
        public IDictionary<TKey, IDictionary<TKey, ICollection<TValue>>> Graph { get; }
    }
}