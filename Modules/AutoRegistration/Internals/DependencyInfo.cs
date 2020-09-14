namespace SpaceEngineers.Core.AutoRegistration.Internals
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using AutoWiringApi.Contexts;
    using AutoWiringApi.Enumerations;
    using Basics;
    using Extensions;
    using SimpleInjector;

    /// <summary>
    /// Dependency information about node in objects graph
    /// </summary>
    [DebuggerDisplay("{ServiceType}")]
    internal class DependencyInfo : IDependencyInfo
    {
        private DependencyInfo(Type serviceType,
                               Type implementationType,
                               uint depth)
        {
            IsCollectionResolvable = CollectionResolvable(serviceType);
            ServiceType = ExtractType(serviceType, IsCollectionResolvable);
            ImplementationType = ExtractType(implementationType, IsCollectionResolvable);

            Dependencies = new List<DependencyInfo>();
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
        public IReadOnlyCollection<IDependencyInfo> Dependencies { get; private set; }

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
        public void TraverseByGraph(Action<IDependencyInfo> action)
        {
            action(this);

            Dependencies.Each(relationship => relationship.TraverseByGraph(action));
        }

        /// <summary>
        /// Retrieve dependency graph from container producer
        /// </summary>
        /// <param name="dependency">Dependency</param>
        /// <param name="visited">Visited nodes</param>
        /// <param name="depth">Current dependency depth in objects graph</param>
        /// <returns>DependencyInfo object</returns>
        internal static DependencyInfo RetrieveDependencyGraph(InstanceProducer dependency,
                                                               IDictionary<InstanceProducer, DependencyInfo> visited,
                                                               uint depth)
        {
            if (visited.TryGetValue(dependency, out var nodeInfo))
            {
                nodeInfo.IsCyclic = true;
                return nodeInfo;
            }

            var lifestyle = new Func<EnLifestyle?>(() => dependency.Lifestyle.MapLifestyle())
                           .Try()
                           .Catch<NotSupportedException>()
                           .Invoke();

            var newNodeInfo = new DependencyInfo(dependency.ServiceType,
                                                 dependency.Registration.ImplementationType,
                                                 depth)
                              {
                                  Lifestyle = lifestyle
                              };

            visited.Add(dependency, newNodeInfo);

            newNodeInfo.Dependencies = dependency
                                      .GetRelationships()
                                      .Select(r => RetrieveDependencyGraph(r.Dependency, visited, depth + 1))
                                      .ToList();

            return newNodeInfo;
        }

        /// <summary>
        /// Retrieve unregistered dependency info
        /// </summary>
        /// <param name="serviceType">Service type</param>
        /// <returns>DependencyInfo object</returns>
        internal static DependencyInfo UnregisteredDependencyInfo(Type serviceType)
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

        private static Type ExtractType(Type type, bool isCollectionResolvable)
        {
            return isCollectionResolvable && type.IsGenericType
                       ? type.GetGenericArguments()[0]
                       : type;
        }
    }
}