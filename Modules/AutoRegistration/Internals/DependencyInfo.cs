namespace SpaceEngineers.Core.AutoRegistration.Internals
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using AutoWiring.Api.Contexts;
    using AutoWiring.Api.Enumerations;
    using Basics;
    using Basics.EqualityComparers;
    using Extensions;
    using SimpleInjector;

    /// <inheritdoc />
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

        /// <inheritdoc />
        public Type ServiceType { get; }

        /// <inheritdoc />
        public Type ImplementationType { get; }

        /// <inheritdoc />
        public EnLifestyle? Lifestyle { get; private set; }

        /// <inheritdoc />
        public IReadOnlyCollection<IDependencyInfo> Dependencies { get; private set; }

        /// <inheritdoc />
        public uint Depth { get; }

        /// <inheritdoc />
        public uint ComplexityDepth
            => Dependencies.Any()
                   ? Dependencies.Max(d => d.ComplexityDepth)
                   : Depth;

        /// <inheritdoc />
        public bool IsCollectionResolvable { get; }

        /// <inheritdoc />
        public bool IsCyclic { get; private set; }

        /// <inheritdoc />
        public bool IsUnregistered { get; private set; }

        /// <inheritdoc />
        public IDependencyInfo? Parent { get; private set; }

        /// <inheritdoc />
        public void TraverseByGraph(Action<IDependencyInfo> action)
        {
            action(this);

            Dependencies.Each(dependency => dependency.TraverseByGraph(action));
        }

        /// <inheritdoc />
        public IEnumerable<T> ExtractFromGraph<T>(Func<IDependencyInfo, T> extractor)
        {
            return new[] { extractor(this) }.Concat(Dependencies.SelectMany(dependency => dependency.ExtractFromGraph(extractor)));
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

        /// <summary>
        /// Retrieve dependency graph from container producer
        /// </summary>
        /// <param name="producer">InstanceProducer</param>
        /// <returns>DependencyInfo object</returns>
        internal static DependencyInfo RetrieveDependencyGraph(InstanceProducer producer)
        {
            var visited = new Dictionary<InstanceProducer, DependencyInfo>(new ReferenceEqualityComparer<InstanceProducer>());
            return RetrieveDependencyGraph(producer, visited, null, 0);
        }

        private static DependencyInfo RetrieveDependencyGraph(
            InstanceProducer dependency,
            IDictionary<InstanceProducer, DependencyInfo> visited,
            IDependencyInfo? parent,
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
                                  Lifestyle = lifestyle,
                                  Parent = parent
                              };

            visited.Add(dependency, newNodeInfo);

            newNodeInfo.Dependencies = dependency
                                      .GetRelationships()
                                      .Select(r => RetrieveDependencyGraph(r.Dependency, visited, newNodeInfo, depth + 1))
                                      .ToList();

            return newNodeInfo;
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