namespace SpaceEngineers.Core.AutoRegistration
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using AutoWiringApi.Enumerations;
    using Basics;
    using SimpleInjector;

    /// <summary>
    /// Dependency information about node in objects graph
    /// </summary>
    [DebuggerDisplay("{ServiceType}")]
    public class DependencyInfo
    {
        private DependencyInfo(Type serviceType,
                               Type componentType,
                               EnLifestyle lifestyle,
                               uint depth,
                               bool isCollectionResolvable)
        {
            ServiceType = serviceType;
            ComponentType = componentType;
            Lifestyle = lifestyle;
            Dependencies = new List<DependencyInfo>();

            Depth = depth;
            IsCollectionResolvable = isCollectionResolvable;
            IsCyclic = false;
        }

        /// <summary>
        /// Service type (interface)
        /// </summary>
        public Type ServiceType { get; }

        /// <summary>
        /// Component type (implementation)
        /// </summary>
        public Type ComponentType { get; }

        /// <summary>
        /// Componemt lifestyle
        /// </summary>
        public EnLifestyle Lifestyle { get; }

        /// <summary>
        /// Component dependencies
        /// </summary>
        public ICollection<DependencyInfo> Dependencies { get; private set; }

        /// <summary>
        /// Component depth in objects graph
        /// </summary>
        public uint Depth { get; }

        /// <summary>
        /// IsCollectionResolvable attribute of service
        /// </summary>
        public bool IsCollectionResolvable { get; }

        /// <summary>
        /// Cyclic dependency attribute
        /// </summary>
        public bool IsCyclic { get; private set; }

        /// <summary>
        /// Execute action on DependencyInfo object and its dependencies
        /// </summary>
        /// <param name="action">Action</param>
        public void ExecuteAction(Action<DependencyInfo> action)
        {
            action(this);

            Dependencies.Each(relationship => relationship.ExecuteAction(action));
        }

        /// <summary>
        /// Retrieve dependency graph from container producer
        /// </summary>
        /// <param name="dependency">Dependency</param>
        /// <param name="visited">Visited nodes</param>
        /// <param name="depth">Current dependency depth in objects graph</param>
        /// <returns>DependencyInfo object</returns>
        public static DependencyInfo RetrieveDependencyGraph(InstanceProducer dependency,
                                                             IDictionary<InstanceProducer, DependencyInfo> visited,
                                                             uint depth)
        {
            if (visited.TryGetValue(dependency, out var nodeInfo))
            {
                nodeInfo.IsCyclic = true;
                return nodeInfo;
            }

            var isCollectionResolvable = dependency.ServiceType
                                                   .GetInterfaces()
                                                   .Contains(typeof(IEnumerable));

            var serviceType = isCollectionResolvable
                                  ? dependency.ServiceType.GetGenericArguments()[0]
                                  : dependency.ServiceType;

            var componentType = isCollectionResolvable
                                    ? dependency.Registration.ImplementationType.GetGenericArguments()[0]
                                    : dependency.Registration.ImplementationType;

            var newNodeInfo = new DependencyInfo(serviceType,
                                                 componentType,
                                                 LifeStyleMapper.MapLifestyle(dependency.Lifestyle),
                                                 depth,
                                                 isCollectionResolvable);

            visited.Add(dependency, newNodeInfo);

            InstanceProducer[] dependencies;

            if (isCollectionResolvable)
            {
                dependencies = dependency.Registration
                                         .GetPropertyValue("Collection")
                                         .GetFieldValue("producers")
                                         .TryExtractFromNullable<IEnumerable>()
                                         .GetEnumerator()
                                         .ToObjectEnumerable()
                                         .Select(o => o.GetPropertyValue("Value"))
                                         .OfType<InstanceProducer>()
                                         .ToArray();
            }
            else
            {
                dependencies = dependency.GetRelationships()
                                         .Select(z => z.Dependency)
                                         .ToArray();
            }

            newNodeInfo.Dependencies = dependencies
                                      .Select(d => RetrieveDependencyGraph(d,
                                                                           visited,
                                                                           depth + 1))
                                      .ToList();

            return newNodeInfo;
        }
    }
}