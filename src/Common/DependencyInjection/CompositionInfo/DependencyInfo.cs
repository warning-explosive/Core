namespace SpaceEngineers.Core.DependencyInjection.CompositionInfo;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SimpleInjector;
using Basics;
using Basics.EqualityComparers;
using Extensions;

[DebuggerDisplay("{ServiceType}")]
public class DependencyInfo
{
    private DependencyInfo(
        InstanceProducer? instanceProducer,
        Type serviceType,
        Type implementationType,
        uint depth)
    {
        InstanceProducer = instanceProducer;
        ServiceType = UnwrapType(serviceType, out var isCollectionResolvable);
        ImplementationType = UnwrapType(implementationType, out _);

        IsCollectionResolvable = isCollectionResolvable;
        IsCyclic = false;
        IsUnregistered = instanceProducer == null;

        Dependencies = new List<DependencyInfo>();
        Depth = depth;
    }

    public Type ServiceType { get; }

    public Type ImplementationType { get; }

    public EnLifestyle? Lifestyle { get; private set; }

    public IReadOnlyCollection<DependencyInfo> Dependencies { get; private set; }

    public uint Depth { get; }

    public uint ComplexityDepth
        => Dependencies.Any()
            ? Dependencies.Max(d => d.ComplexityDepth)
            : Depth;

    public bool IsCollectionResolvable { get; }

    public bool IsCyclic { get; private set; }

    public bool IsUnregistered { get; }

    public DependencyInfo? Parent { get; private set; }

    internal InstanceProducer? InstanceProducer { get; }

    public void TraverseByGraph(Action<DependencyInfo> action)
    {
        action(this);

        Dependencies.Each(dependency => dependency.TraverseByGraph(action));
    }

    public IEnumerable<T> ExtractFromGraph<T>(Func<DependencyInfo, T> extractor)
    {
        return this
            .Flatten(info => info.Dependencies)
            .Select(extractor);
    }

    internal static DependencyInfo UnregisteredDependencyInfo(Type serviceType)
    {
        return new DependencyInfo(null, serviceType, serviceType, 0);
    }

    internal static DependencyInfo RetrieveDependencyGraph(InstanceProducer producer)
    {
        var visited = new Dictionary<InstanceProducer, DependencyInfo>(new ReferenceEqualityComparer<InstanceProducer>());
        return RetrieveDependencyGraph(producer, visited, null, 0);
    }

    private static DependencyInfo RetrieveDependencyGraph(
        InstanceProducer dependency,
        IDictionary<InstanceProducer, DependencyInfo> visited,
        DependencyInfo? parent,
        uint depth)
    {
        if (visited.TryGetValue(dependency, out var nodeInfo))
        {
            nodeInfo.IsCyclic = true;
            return nodeInfo;
        }

        var lifestyle = ExecutionExtensions
            .Try<Lifestyle, EnLifestyle?>(static lifestyle => lifestyle.MapLifestyle(), dependency.Lifestyle)
            .Catch<NotSupportedException>()
            .Invoke(_ => default(EnLifestyle?));

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

    private static Type UnwrapType(Type type, out bool isCollectionResolvable)
    {
        isCollectionResolvable = type.GetInterfaces().Contains(typeof(IEnumerable));

        var unwrappedCollection = isCollectionResolvable && type.IsGenericType
            ? type.GetGenericArguments()[0]
            : type;

        return unwrappedCollection;
    }
}