namespace SpaceEngineers.Core.CompositionRoot
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using Enumerations;
    using Extensions;
    using SimpleInjector;

    [DebuggerDisplay("{ServiceType}")]
    public class DependencyInfo
    {
        private DependencyInfo(Type serviceType,
                               Type componentType,
                               EnLifestyle enLifestyle,
                               uint depth,
                               bool isCollectionResolvable)
        {
            ServiceType = serviceType;
            ComponentType = componentType;
            EnLifestyle = enLifestyle;
            Relationships = new List<DependencyInfo>();

            Depth = depth;
            IsCollectionResolvable = isCollectionResolvable;
            IsCyclic = false;
        }

        public Type ServiceType { get; }

        public Type ComponentType { get; }

        public EnLifestyle EnLifestyle { get; }

        public ICollection<DependencyInfo> Relationships { get; private set; }

        public uint Depth { get; }

        public bool IsCollectionResolvable { get; }

        public bool IsCyclic { get; private set; }

        public void ExecuteAction(Action<DependencyInfo> action)
        {
            action(this);

            Relationships.Each(relationship => relationship.ExecuteAction(action));
        }

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

            newNodeInfo.Relationships = (isCollectionResolvable
                                             ? dependency.Registration.GetRelationships()
                                             : dependency.GetRelationships())
                                       .Select(relationship => RetrieveDependencyGraph(relationship.Dependency,
                                                                                       visited,
                                                                                       depth + 1))
                                       .ToList();

            return newNodeInfo;
        }
    }
}