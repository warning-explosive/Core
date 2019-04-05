namespace SpaceEngineers.Core.Utilities.Services.Implementations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Extensions;

    /// <summary>
    /// Path resolver
    /// Filters
    /// </summary>
    public partial class PathResolverServiceImpl<TKey, TValue>
        where TKey : struct
        where TValue : IEquatable<TValue>
    {
        private static bool FilterGroupedPathByTargetKey(Queue<KeyValuePair<TKey, ICollection<TValue>>> groupedPath,
                                                         TKey targetNodeKey)
        {
            return EqualityComparer<TKey>.Default.Equals(groupedPath.Last().Key,
                                                         targetNodeKey);
        }

        private static bool FilterGroupedPathByLoops(Queue<KeyValuePair<TKey, ICollection<TValue>>> groupedPath,
                                                     bool withOutLoops)
        {
            if (withOutLoops)
            {
                var groupedPathArray = groupedPath.ToArray();
                var looopsExist = false;
            
                for (var i = 1; i < groupedPathArray.Length; ++i)
                {
                    looopsExist = EqualityComparer<TKey>.Default.Equals(groupedPathArray[i].Key, groupedPathArray[i - 1].Key);
                }

                if (looopsExist)
                {
                    return false;
                }
            }

            return true;
        }

        private static bool FilterGroupedPathByRequiredKeys(Queue<KeyValuePair<TKey, ICollection<TValue>>> groupedPath,
                                                            Queue<TKey> requiredKeys = null)
        {
            if (requiredKeys == null || !requiredKeys.Any())
            {
                return true;
            }

            var localGroupedPath = groupedPath.DeepCopy();
            var localRequiredKeys = requiredKeys.DeepCopy();
            
            var localRequiredkey = localRequiredKeys.Dequeue();
                
            while (localGroupedPath.Any())
            {
                var nodeGroup = localGroupedPath.Dequeue();

                if (EqualityComparer<TKey>.Default.Equals(nodeGroup.Key, localRequiredkey))
                {
                    if (localRequiredKeys.Any())
                    {
                        localRequiredkey = localRequiredKeys.Dequeue();
                    }
                    else
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool FilterGroupedPathByRequiredEdges(Queue<KeyValuePair<TKey, ICollection<TValue>>> groupedPath,
                                                             Queue<TValue> requiredEdges)
        {
            if (requiredEdges == null || !requiredEdges.Any())
            {
                return true;
            }

            var localGroupedPath = groupedPath.DeepCopy();
            var localRequiredEdges = requiredEdges.DeepCopy();

            var localRequiredEdge = localRequiredEdges.Dequeue();
            var nodeGroup = localGroupedPath.Peek();
            var nextGroup = true;

            while (localGroupedPath.Any())
            {
                if (nextGroup)
                {
                    nodeGroup = localGroupedPath.Dequeue();
                }
                
                if (nodeGroup.Value.Any(edge => edge.Equals(localRequiredEdge)))
                {
                    nodeGroup.Value.Remove(localRequiredEdge);
                    nextGroup = false;
                    
                    if (localRequiredEdges.Any())
                    {
                        localRequiredEdge = localRequiredEdges.Dequeue();
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    nextGroup = true;
                }
            }

            return false;
        }

        private static Queue<KeyValuePair<TKey, ICollection<TValue>>> ExtractPathsFromGroups(Queue<KeyValuePair<TKey, ICollection<TValue>>> groupedPath,
                                                                                             Queue<TValue> requiredEdges)
        {
            if (requiredEdges == null || !requiredEdges.Any())
            {
                return groupedPath;
            }

            var localGroupedPath = groupedPath.DeepCopy();
            var localRequiredEdges = requiredEdges.DeepCopy();

            var resultGroupedPath = new Queue<KeyValuePair<TKey, ICollection<TValue>>>();
            
            var nodeGroup = localGroupedPath.Peek();
            var nextGroup = true;
            var forcePullRemains = false;
            
            var localRequiredEdge = localRequiredEdges.Dequeue();

            while (localGroupedPath.Any())
            {
                if (nextGroup)
                {
                    nodeGroup = localGroupedPath.Dequeue();
                }
                
                if (!forcePullRemains
                    && nodeGroup.Value.Any(edge => edge.Equals(localRequiredEdge)))
                {
                    nodeGroup.Value.Remove(localRequiredEdge);
                    nextGroup = false;
                    resultGroupedPath.Enqueue(new KeyValuePair<TKey, ICollection<TValue>>(nodeGroup.Key, new List<TValue> { localRequiredEdge } ));
                    
                    if (localRequiredEdges.Any())
                    {
                        localRequiredEdge = localRequiredEdges.Dequeue();
                    }
                    else
                    {
                        forcePullRemains = true;
                    }
                }
                else
                {
                    if (nextGroup)
                    {
                        resultGroupedPath.Enqueue(nodeGroup);
                    }
                    
                    nextGroup = true;
                }
            }

            return localRequiredEdges.Any()
                       ? null
                       : resultGroupedPath;
        }
    }
}