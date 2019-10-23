namespace SpaceEngineers.Core.Utilities.PathResolver
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Abstractions;
    using CompositionRoot;
    using CompositionRoot.Attributes;
    using CompositionRoot.Enumerations;
    using CompositionRoot.Extensions;

    /// <inheritdoc />
    [Lifestyle(lifestyle: EnLifestyle.Singleton)]
    internal partial class PathResolverImpl<TKey, TValue> : IPathResolver<TKey, TValue>
        where TKey : struct
        where TValue : IEquatable<TValue>
    {
        private static readonly string NotFound = "Path not found";

        private static readonly string AmbiguousMatch = "Ambiguous number of paths";

        private static readonly Func<PathResolverInfo<TKey, TValue>, string> _additionalInfo =
            gsf => gsf.ShowProperties(BindingFlags.Instance | BindingFlags.Public,
                                      nameof(PathResolverInfo<TKey, TValue>.WeightFunc));

        /// <inheritdoc />
        public Queue<KeyValuePair<TKey, TValue>> GetShortestPath(GenericGraph<TKey, TValue> genericGraph,
                                                                 PathResolverInfo<TKey, TValue> pathResolverInfo)
        {
            if (EqualityComparer<TKey>.Default.Equals(pathResolverInfo.RootNodeKey, pathResolverInfo.TargetNodeKey)
                && !pathResolverInfo.NotEmptyCircle
                && (pathResolverInfo.RequiredKeys == null || !pathResolverInfo.RequiredKeys.Any())
                && (pathResolverInfo.RequiredEdges == null || !pathResolverInfo.RequiredEdges.Any()))
            {
                return new Queue<KeyValuePair<TKey, TValue>>();
            }

            var query = GetAllGroupedPaths(genericGraph, pathResolverInfo.RootNodeKey)
                       .Where(grpdPath => FilterGroupedPathByTargetKey(grpdPath, pathResolverInfo.TargetNodeKey))
                       .Where(grpdPath => FilterGroupedPathByRequiredKeys(grpdPath, pathResolverInfo.RequiredKeys))
                       .Where(grpdPath => FilterGroupedPathByLoops(grpdPath, pathResolverInfo.WithoutLoops))
                       .Where(grpdPath => FilterGroupedPathByRequiredEdges(grpdPath, pathResolverInfo.RequiredEdges))
                       .Select(grpdPath => ExtractPathsFromGroups(grpdPath, pathResolverInfo.RequiredEdges));

            var groupedPaths = SelectShortestGroupedPath(query, pathResolverInfo.WeightFunc).ToArray();

            if (groupedPaths.Length < 1)
            {
                throw new Exception(NotFound + "\n" + _additionalInfo(pathResolverInfo));
            }
            
            if (groupedPaths.Length > 1)
            {
                var strPaths = string.Join("\n", groupedPaths.Select(grpPath => PrintSingleGroupedPath(grpPath, pathResolverInfo.WeightFunc)));
                throw new AmbiguousMatchException(AmbiguousMatch + "\n" + strPaths + "\n" + _additionalInfo(pathResolverInfo));
            }

            var groupedPath = groupedPaths.Single();

            if (groupedPath.Any(nodeGroup => nodeGroup.Value.Count > 1))
            {
                var strPath = PrintSingleGroupedPath(groupedPath, pathResolverInfo.WeightFunc);
                throw new AmbiguousMatchException(AmbiguousMatch + "\n" + strPath + "\n" + _additionalInfo(pathResolverInfo));
            }

            var resultPath = new Queue<KeyValuePair<TKey, TValue>>();
            
            foreach (var nodeGroup in groupedPath)
            {
                resultPath.Enqueue(new KeyValuePair<TKey, TValue>(nodeGroup.Key, nodeGroup.Value.Single()));
            }
            
            return resultPath;
        }

        /// <inheritdoc />
        public IEnumerable<Queue<KeyValuePair<TKey, ICollection<TValue>>>> GetAllGroupedPaths(
            GenericGraph<TKey, TValue> genericGraph,
            TKey rootNodeKey)
        {
            var groupedPathsCollection = new List<Queue<KeyValuePair<TKey, ICollection<TValue>>>>();

            ProcessSingleNodeRecoursive(genericGraph,
                                        rootNodeKey,
                                        new Queue<KeyValuePair<TKey, ICollection<TValue>>>(),
                                        new HashSet<TKey>(),
                                        groupedPathsCollection,
                                        true);

            return groupedPathsCollection;
        }

        /// <inheritdoc />
        public IEnumerable<KeyValuePair<int, Queue<KeyValuePair<TKey, ICollection<TValue>>>>> GetAllGroupedWeightedPaths(
            GenericGraph<TKey, TValue> genericGraph,
            TKey rootKey,
            Func<TValue, int> weightFunc)
        {
            return GetAllGroupedPaths(genericGraph, rootKey)
               .Select(groupedPath => new KeyValuePair<int, Queue<KeyValuePair<TKey, ICollection<TValue>>>>(WeightGroupedPath(groupedPath, weightFunc),
                                                                                                            groupedPath));
        }

        private static void ProcessSingleNodeRecoursive(GenericGraph<TKey, TValue> genericGraph,
                                                        TKey currentNodeKey,
                                                        Queue<KeyValuePair<TKey, ICollection<TValue>>> visitedNodes,
                                                        HashSet<TKey> visitedNodesKeys,
                                                        ICollection<Queue<KeyValuePair<TKey, ICollection<TValue>>>> weightedPathsCollection,
                                                        bool isFirstLaunch)
        {
            if (!genericGraph.Graph.TryGetValue(currentNodeKey, out var nearestUnvisitedNodes))
            {
                // nearest nodes is not available => deadlock
                return;
            }
            
            foreach (var nearestUnvisitedNode in nearestUnvisitedNodes.Where(z => !visitedNodesKeys.Contains(z.Key)))
            {
                var localVisited = visitedNodes.DeepCopy();
                var localVisitedKeys = visitedNodesKeys.DeepCopy();

                localVisited.Enqueue(nearestUnvisitedNode);

                if (!isFirstLaunch)
                {
                    localVisitedKeys.Add(currentNodeKey);
                }
                
                weightedPathsCollection.Add(localVisited);
                
                ProcessSingleNodeRecoursive(genericGraph,
                                            nearestUnvisitedNode.Key,
                                            localVisited,
                                            localVisitedKeys,
                                            weightedPathsCollection,
                                            false);
            }
        }

        private static IEnumerable<Queue<KeyValuePair<TKey, ICollection<TValue>>>> SelectShortestGroupedPath(IEnumerable<Queue<KeyValuePair<TKey, ICollection<TValue>>>> groupedPaths,
                                                                                                             Func<TValue, int> weightFunc)
        {
            var shortestGroupedPaths = groupedPaths
                                      .Select(groupedPath => new
                                                             {
                                                                 PathWeight = WeightGroupedPath(groupedPath, weightFunc),
                                                                 Path = groupedPath
                                                             })
                                      .GroupBy(z => z.PathWeight)
                                      .OrderBy(z => z.Key)
                                      .FirstOrDefault();

            return shortestGroupedPaths == null
                       ? Enumerable.Empty<Queue<KeyValuePair<TKey, ICollection<TValue>>>>()
                       : shortestGroupedPaths.Select(z => z.Path);
        }

        private static int WeightGroupedPath(Queue<KeyValuePair<TKey, ICollection<TValue>>> groupedPath,
                                             Func<TValue, int> weightFunc)
        {
            return groupedPath.Sum(nodeGroup => nodeGroup.Value.Min(weightFunc));
        }

        private static string PrintSingleGroupedPath(Queue<KeyValuePair<TKey, ICollection<TValue>>> groupedPath,
                                                     Func<TValue, int> weightFunc)
        {
            return $"({WeightGroupedPath(groupedPath, weightFunc)}) => "
                   + string.Join(" => ",
                                 groupedPath.Select(nodeGroup => "["
                                                                 + string.Join(", ",
                                                                               nodeGroup.Value.Select(edge => edge.ToString() + $"({weightFunc(edge)})"))
                                                                 + "]"));
        }
    }
}