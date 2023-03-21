namespace SpaceEngineers.Core.Basics
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using Attributes;

    internal sealed class TypeInfoStorage
    {
        private static readonly ConcurrentDictionary<string, TypeInfo> Cache
            = new ConcurrentDictionary<string, TypeInfo>();

        private static readonly Lazy<IReadOnlyDictionary<string, IReadOnlyDictionary<string, Type>>> TypesCache
            = new Lazy<IReadOnlyDictionary<string, IReadOnlyDictionary<string, Type>>>(InitializeTypesCache, LazyThreadSafetyMode.ExecutionAndPublication);

        private static readonly Lazy<IReadOnlyDictionary<Type, IReadOnlyCollection<Type>>> BeforeAfterAttributesMap
            = new Lazy<IReadOnlyDictionary<Type, IReadOnlyCollection<Type>>>(InitializeBeforeAfterAttributesMap, LazyThreadSafetyMode.ExecutionAndPublication);

        internal static bool TryGet(string assemblyName, string typeFullName, [NotNullWhen(true)] out Type? type)
        {
            if (TypesCache.Value.TryGetValue(assemblyName, out var types)
                && types.TryGetValue(typeFullName, out type))
            {
                return true;
            }

            type = default;
            return false;
        }

        internal static TypeInfo Get(Type type) => Cache.GetOrAdd(GetKey(type), static (_, t) => new TypeInfo(t), type);

        internal static IEnumerable<Type> ExtractDependencies(Type type)
        {
            var byAfterAttribute = type
                 .GetCustomAttribute<AfterAttribute>()
                ?.Types ?? Enumerable.Empty<Type>();

            var byBeforeAttribute = BeforeAfterAttributesMap.Value.TryGetValue(type, out var value)
                ? value
                : Enumerable.Empty<Type>();

            return byAfterAttribute.Concat(byBeforeAttribute);
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

        private static IReadOnlyDictionary<string, IReadOnlyDictionary<string, Type>> InitializeTypesCache()
        {
            return AssembliesExtensions
               .AllAssembliesFromCurrentDomain()
               .ToDictionary(
                    assembly => assembly.GetName().Name,
                    assembly => (IReadOnlyDictionary<string, Type>)assembly
                       .GetTypes()
                       .ToDictionary(type => type.FullName));
        }

        private static IReadOnlyDictionary<Type, IReadOnlyCollection<Type>> InitializeBeforeAfterAttributesMap()
        {
            return AssembliesExtensions
                .AllAssembliesFromCurrentDomain()
                .Where(assembly => !assembly.IsDynamic)
                .SelectMany(GetTypes)
                .Select(type =>
                {
                    var attribute = type.GetCustomAttribute<BeforeAttribute>();
                    return (type, attribute);
                })
                .Where(pair => pair.attribute != null)
                .SelectMany(pair => pair.attribute.Types.Select(before => (before, pair.type)))
                .GroupBy(pair => pair.before, pair => pair.type)
                .ToDictionary(grp => grp.Key, grp => grp.ToList() as IReadOnlyCollection<Type>);

            static IEnumerable<Type> GetTypes(Assembly assembly)
            {
                return ExecutionExtensions
                    .Try<IEnumerable<Type>>(assembly.GetTypes)
                    .Catch<ReflectionTypeLoadException>()
                    .Catch<FileNotFoundException>()
                    .Invoke(_ => Enumerable.Empty<Type>());
            }
        }
    }
}