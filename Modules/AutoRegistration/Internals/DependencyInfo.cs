namespace SpaceEngineers.Core.AutoRegistration.Internals
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using AutoWiring.Api.Abstractions;
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
        private DependencyInfo(
            InstanceProducer? instanceProducer,
            Type serviceType,
            Type implementationType,
            uint depth)
        {
            InstanceProducer = instanceProducer;
            ServiceType = UnwrapType(serviceType, out var isCollectionResolvable, out var isVersioned);
            ImplementationType = UnwrapType(implementationType, out _, out _);

            IsCollectionResolvable = isCollectionResolvable;
            IsVersioned = isVersioned;
            IsCyclic = false;
            IsUnregistered = instanceProducer == null;

            Dependencies = new List<DependencyInfo>();
            Depth = depth;
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
        public bool IsVersioned { get; }

        /// <inheritdoc />
        public bool IsCyclic { get; private set; }

        /// <inheritdoc />
        public bool IsUnregistered { get; }

        /// <inheritdoc />
        public IDependencyInfo? Parent { get; private set; }

        internal InstanceProducer? InstanceProducer { get; }

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
            return new DependencyInfo(null, serviceType, serviceType, 0);
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

            var newNodeInfo = new DependencyInfo(
                dependency,
                dependency.ServiceType,
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

        private static Type UnwrapType(Type type, out bool isCollectionResolvable, out bool isVersioned)
        {
            isCollectionResolvable = type.GetInterfaces().Contains(typeof(IEnumerable));

            var unwrappedCollection = isCollectionResolvable && type.IsGenericType
                ? type.GetGenericArguments()[0]
                : type;

            isVersioned = unwrappedCollection.IsSubclassOfOpenGeneric(typeof(IVersioned<>));

            return unwrappedCollection;
        }
    }
}