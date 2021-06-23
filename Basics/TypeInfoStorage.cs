namespace SpaceEngineers.Core.Basics
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using Attributes;

    /// <summary>
    /// TypeInfo storage
    /// </summary>
    public sealed class TypeInfoStorage
    {
        private static readonly ConcurrentDictionary<string, ITypeInfo> Cache = new ConcurrentDictionary<string, ITypeInfo>();

        private static readonly Lazy<IReadOnlyDictionary<Type, IReadOnlyCollection<Type>>> DependentsMap
            = new Lazy<IReadOnlyDictionary<Type, IReadOnlyCollection<Type>>>(InitializeDependents, LazyThreadSafetyMode.ExecutionAndPublication);

        /// <summary>
        /// Gets or builds type info from storage
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>ITypeInfo</returns>
        public static ITypeInfo Get(Type type) => Cache.GetOrAdd(GetKey(type), _ => new TypeInfo(type));

        internal static IReadOnlyCollection<Type> ExtractDependencies(Type type)
        {
            var byDependencyAttribute = type.GetCustomAttribute<DependencyAttribute>()?.Dependencies
                                        ?? Enumerable.Empty<Type>();

            var byDependentAttribute = DependentsMap.Value.TryGetValue(type, out var value)
                ? value
                : Enumerable.Empty<Type>();

            return byDependencyAttribute
                .Concat(byDependentAttribute)
                .ToList();
        }

        private static string GetKey(Type type)
        {
            if (type.IsGenericParameter)
            {
                throw new InvalidOperationException("Type cache doesn't support generic parameters");
            }

            return !type.IsGenericTypeDefinition && type.ContainsGenericParameters
                ? type.ToString()
                : type.FullName ?? throw new InvalidOperationException($"Type cache doesn't support types without {nameof(Type.FullName)}: {type}");
        }

        private static IReadOnlyDictionary<Type, IReadOnlyCollection<Type>> InitializeDependents()
        {
            return AssembliesExtensions
                .AllFromCurrentDomain()
                .Where(assembly => !assembly.IsDynamic)
                .SelectMany(assembly => new Func<IEnumerable<Type>>(assembly.GetTypes)
                    .Try()
                    .Catch<ReflectionTypeLoadException>()
                    .Invoke(_ => Enumerable.Empty<Type>()))
                .Select(type =>
                {
                    var attribute = type.GetCustomAttribute<DependentAttribute>();
                    return (type, attribute);
                })
                .Where(pair => pair.attribute != null)
                .SelectMany(pair => pair.attribute.Dependents.Select(dependent => (dependent, pair.type)))
                .GroupBy(pair => pair.dependent, pair => pair.type)
                .ToDictionary(grp => grp.Key, grp => grp.ToList() as IReadOnlyCollection<Type>);
        }
    }
}