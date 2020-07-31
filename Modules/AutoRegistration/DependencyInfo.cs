namespace SpaceEngineers.Core.AutoRegistration
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using AutoWiringApi.Enumerations;
    using Basics;
    using Internals;
    using SimpleInjector;

    /// <summary>
    /// Dependency information about node in objects graph
    /// </summary>
    [DebuggerDisplay("{ServiceType}")]
    public class DependencyInfo
    {
        private DependencyInfo(Type serviceType,
                               Type implementationType,
                               uint depth)
        {
            ServiceType = serviceType;
            ImplementationType = implementationType;
            Dependencies = new List<DependencyInfo>();

            IsCollectionResolvable = CollectionResolvable(serviceType);
            Depth = depth;
            IsCyclic = false;
        }

        /// <summary>
        /// Service type
        /// </summary>
        public Type ServiceType { get; }

        /// <summary>
        /// Implementation type
        /// </summary>
        public Type ImplementationType { get; }

        /// <summary>
        /// Component lifestyle
        /// </summary>
        public EnLifestyle? Lifestyle { get; private set; }

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
        /// IsUnregistered attribute
        /// </summary>
        public bool IsUnregistered { get; private set; }

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

            var isCollectionResolvable = CollectionResolvable(dependency.ServiceType);

            var serviceType = isCollectionResolvable
                                  ? dependency.ServiceType.GetGenericArguments()[0]
                                  : dependency.ServiceType;

            var implementationType = isCollectionResolvable
                                         ? dependency.Registration.ImplementationType.GetGenericArguments()[0]
                                         : dependency.Registration.ImplementationType;

            var lifestyle = new Func<EnLifestyle?>(() => dependency.Lifestyle.MapLifestyle())
                           .Try()
                           .Catch<NotSupportedException>()
                           .Invoke();

            var newNodeInfo = new DependencyInfo(serviceType, implementationType, depth)
                              {
                                  Lifestyle = lifestyle
                              };

            visited.Add(dependency, newNodeInfo);

            newNodeInfo.Dependencies = dependency.GetRelationships()
                                                 .Select(r => RetrieveDependencyGraph(r.Dependency, visited, depth + 1))
                                                 .ToList();

            return newNodeInfo;
        }

        /// <summary>
        /// Retrieve unregistered dependency info
        /// </summary>
        /// <param name="serviceType">Service type</param>
        /// <returns>DependencyInfo object</returns>
        public static DependencyInfo UnregisteredDependencyInfo(Type serviceType)
        {
            return new DependencyInfo(serviceType, serviceType, 0)
                   {
                       IsUnregistered = true
                   };
        }

        private static bool CollectionResolvable(Type serviceType)
        {
            return serviceType.GetInterfaces().Contains(typeof(IEnumerable));
        }
    }
}